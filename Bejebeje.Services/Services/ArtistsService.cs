namespace Bejebeje.Services.Services
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Common.Extensions;
  using Bejebeje.DataAccess.Context;
  using Bejebeje.Domain;
  using Bejebeje.Models.Artist;
  using Bejebeje.Models.Lyric;
  using Bejebeje.Models.Paging;
  using Bejebeje.Services.Config;
  using Bejebeje.Services.Extensions;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Options;
  using Models.Search;
  using NodaTime;
  using Npgsql;

  public class ArtistsService : IArtistsService
  {
    private readonly DatabaseOptions databaseOptions;

    private readonly IArtistSlugsService artistSlugsService;

    private readonly BbContext context;

    private readonly TextInfo textInfo = new CultureInfo("ku-TR", false).TextInfo;

    public ArtistsService(
      IOptionsMonitor<DatabaseOptions> optionsAccessor,
      IArtistSlugsService artistSlugsService,
      BbContext context)
    {
      this.databaseOptions = optionsAccessor.CurrentValue;
      this.artistSlugsService = artistSlugsService;
      this.context = context;
    }

    public async Task<int> GetArtistIdAsync(
      string artistSlug)
    {
      int? artistId = await context
        .Artists
        .AsNoTracking()
        .Where(x => x.Slugs.Any(y => y.Name == artistSlug.Standardize()) && x.IsApproved && !x.IsDeleted)
        .Select(x => (int?)x.Id)
        .FirstOrDefaultAsync();

      if (artistId == null)
      {
        throw new ArtistNotFoundException(artistSlug);
      }

      return artistId.Value;
    }

    public async Task<bool> ArtistExistsAsync(
      string artistSlug)
    {
      int? artistId = await context
        .Artists
        .AsNoTracking()
        .Where(x => x.Slugs.Any(y => y.Name == artistSlug.Standardize()))
        .Select(x => (int?)x.Id)
        .FirstOrDefaultAsync();

      if (artistId == null)
      {
        return false;
      }

      return true;
    }

    public async Task<ArtistViewModel> GetArtistDetailsAsync(
      string artistSlug)
    {
      ArtistViewModel artistViewModel = new ArtistViewModel();

      await using NpgsqlConnection connection = new NpgsqlConnection(databaseOptions.ConnectionString);
      await connection.OpenAsync();

      string sqlCommand = @"select a.id as artist_id, a.first_name, a.last_name, aslug.name as artist_slug, images.data as image_data, a.created_at, a.modified_at from artists as a inner join artist_slugs as aslug on aslug.artist_id = a.id left join artist_images as images on images.artist_id = a.id where aslug.artist_id = (select artist_id from artist_slugs where name = @artist_slug) and aslug.is_primary = true order by a.first_name asc;";

      await using NpgsqlCommand command = new NpgsqlCommand(sqlCommand, connection);

      command.Parameters.AddWithValue("@artist_slug", artistSlug);

      await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        int artistId = Convert.ToInt32(reader[0]);
        string firstName = textInfo.ToTitleCase(Convert.ToString(reader[1]));
        string lastName = textInfo.ToTitleCase(Convert.ToString(reader[2]));
        string artistPrimarySlug = Convert.ToString(reader[3]);
        bool artistHasImage = reader[4] != System.DBNull.Value;
        DateTime createdAt = Convert.ToDateTime(reader[5]);
        DateTime? modifiedAt = reader[6] == System.DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader[6]);

        artistViewModel.Id = artistId;
        artistViewModel.FirstName = firstName;
        artistViewModel.LastName = lastName;
        artistViewModel.FullName = $"{firstName} {lastName}";
        artistViewModel.PrimarySlug = artistPrimarySlug;
        artistViewModel.HasImage = artistHasImage;
        artistViewModel.CreatedAt = createdAt;
        artistViewModel.ModifiedAt = modifiedAt;
      }

      return artistViewModel;
    }

    public async Task<CreateNewArtistResponse> CreateNewArtistAsync(
      CreateNewArtistRequest request)
    {
      string artistFullName = string.IsNullOrEmpty(request.LastName) ? request.FirstName : $"{request.FirstName} {request.LastName}";

      string artistSlug = artistSlugsService.GetArtistSlug(artistFullName);

      bool artistExists = await ArtistExistsAsync(artistSlug);

      if (artistExists)
      {
        throw new ArtistExistsException(artistSlug);
      }

      Artist artist = new Artist
      {
        FirstName = request.FirstName,
        LastName = request.LastName,
        FullName = artistFullName,
        Slugs = new List<ArtistSlug> { artistSlugsService.BuildArtistSlug(artistFullName) },
        CreatedAt = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc(),
      };

      context.Artists.Add(artist);
      await context.SaveChangesAsync();

      CreateNewArtistResponse response = new CreateNewArtistResponse
      {
        Slug = artistSlug,
        CreatedAt = artist.CreatedAt,
      };

      return response;
    }

    public async Task<IEnumerable<SearchArtistResultViewModel>> SearchArtistsAsync(
      string artistName)
    {
      string artistNameStandardized = artistName.NormalizeStringForUrl();

      List<SearchArtistResultViewModel> artists = new List<SearchArtistResultViewModel>();

      await using NpgsqlConnection connection = new NpgsqlConnection(databaseOptions.ConnectionString);
      await connection.OpenAsync();

      await using NpgsqlCommand command = new NpgsqlCommand("select a.first_name, a.last_name, \"as\".name primary_slug, ai.id image_id from artists a inner join artist_slugs \"as\" on a.id = \"as\".artist_id left join artist_images ai on a.id = ai.artist_id where a.id in (select distinct artist_id from artist_slugs where name like @artist_name) and a.is_approved = true and a.is_deleted = false and \"as\".is_primary = true;", connection);

      command.Parameters.AddWithValue("@artist_name", $"%{artistNameStandardized}%");

      await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        SearchArtistResultViewModel artist = new SearchArtistResultViewModel();
        string name = textInfo.ToTitleCase(Convert.ToString(reader[0]) + " " + Convert.ToString(reader[1]));
        string primarySlug = Convert.ToString(reader[2]);
        bool hasImage = reader[3] != System.DBNull.Value;

        artist.Name = name;
        artist.PrimarySlug = primarySlug;
        artist.HasImage = hasImage;

        artists.Add(artist);
      }

      return artists;
    }
  }
}
