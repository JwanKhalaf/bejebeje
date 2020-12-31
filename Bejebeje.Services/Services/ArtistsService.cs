namespace Bejebeje.Services.Services
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Common.Extensions;
  using Common.Enums;
  using Common.Exceptions;
  using Common.Helpers;
  using Config;
  using DataAccess.Context;
  using Domain;
  using Interfaces;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Options;
  using Models.Artist;
  using Models.Search;
  using NodaTime;
  using Npgsql;

  public class ArtistsService : IArtistsService
  {
    private readonly DatabaseOptions _databaseOptions;

    private readonly IArtistSlugsService _artistSlugsService;

    private readonly BbContext _context;

    private readonly TextInfo _textInfo = new CultureInfo("ku-TR", false).TextInfo;

    public ArtistsService(
      IOptionsMonitor<DatabaseOptions> optionsAccessor,
      IArtistSlugsService artistSlugsService,
      BbContext context)
    {
      _databaseOptions = optionsAccessor.CurrentValue;
      _artistSlugsService = artistSlugsService;
      _context = context;
    }

    public async Task<int> GetArtistIdAsync(
      string artistSlug)
    {
      int? artistId = await _context
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
      int? artistId = await _context
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

    public async Task<IEnumerable<ArtistItemViewModel>> GetTopTenFemaleArtistsByLyricsCountAsync()
    {
      List<ArtistItemViewModel> femaleArtists = new List<ArtistItemViewModel>();

      await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
      await connection.OpenAsync();

      string sqlCommand = @"select a.id, a.first_name, a.last_name, s.""name"", a.has_image, count(l.id) as number_of_lyrics from artists a left join lyrics l on a.id = l.artist_id inner join artist_slugs s on s.artist_id = a.id where a.sex = 'f' and s.is_primary = true group by a.id, s.""name"" order by number_of_lyrics desc limit 10;";

      await using NpgsqlCommand command = new NpgsqlCommand(sqlCommand, connection);

      await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        int id = Convert.ToInt32(reader[0]);
        string firstName = _textInfo.ToTitleCase(Convert.ToString(reader[1])).Trim();
        string lastName = _textInfo.ToTitleCase(Convert.ToString(reader[2])).Trim();
        string primarySlug = Convert.ToString(reader[3]);
        bool hasImage = Convert.ToBoolean(reader[4]);

        string artistFullName = $"{firstName} {lastName}".Trim();

        string artistImageUrl = ImageUrlBuilder
          .BuildImageUrl(hasImage, primarySlug, id, ImageSize.Standard);

        string artistImageAlternateText = ImageUrlBuilder
          .GetImageAlternateText(hasImage, artistFullName);

        ArtistItemViewModel artist = new ArtistItemViewModel();

        artist.FirstName = firstName;
        artist.LastName = lastName;
        artist.PrimarySlug = primarySlug;
        artist.ImageUrl = artistImageUrl;
        artist.ImageAlternateText = artistImageAlternateText;

        femaleArtists.Add(artist);
      }

      return femaleArtists;
    }

    public async Task<ArtistViewModel> GetArtistDetailsAsync(
      string artistSlug)
    {
      ArtistViewModel artistViewModel = new ArtistViewModel();

      await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
      await connection.OpenAsync();

      string sqlCommand = @"select a.id as artist_id, a.first_name, a.last_name, aslug.name as artist_slug, a.has_image, a.created_at, a.modified_at from artists as a inner join artist_slugs as aslug on aslug.artist_id = a.id where aslug.artist_id = (select artist_id from artist_slugs where name = @artist_slug) and aslug.is_primary = true order by a.first_name asc;";

      await using NpgsqlCommand command = new NpgsqlCommand(sqlCommand, connection);

      command.Parameters.AddWithValue("@artist_slug", artistSlug);

      await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        int artistId = Convert.ToInt32(reader[0]);
        string firstName = _textInfo.ToTitleCase(Convert.ToString(reader[1])).Trim();
        string lastName = _textInfo.ToTitleCase(Convert.ToString(reader[2])).Trim();
        string artistPrimarySlug = Convert.ToString(reader[3]);
        bool artistHasImage = Convert.ToBoolean(reader[4]);
        DateTime createdAt = Convert.ToDateTime(reader[5]);
        DateTime? modifiedAt = reader[6] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader[6]);

        string artistFullName = $"{firstName} {lastName}".Trim();

        string artistImageUrl = ImageUrlBuilder
          .BuildImageUrl(artistHasImage, artistPrimarySlug, artistId, ImageSize.Standard);

        string artistImageAlternateText = ImageUrlBuilder
          .GetImageAlternateText(artistHasImage, artistFullName);

        artistViewModel.Id = artistId;
        artistViewModel.FirstName = firstName;
        artistViewModel.LastName = lastName;
        artistViewModel.FullName = $"{firstName} {lastName}";
        artistViewModel.PrimarySlug = artistPrimarySlug;
        artistViewModel.ImageUrl = artistImageUrl;
        artistViewModel.ImageAlternateText = artistImageAlternateText;
        artistViewModel.CreatedAt = createdAt;
        artistViewModel.ModifiedAt = modifiedAt;
      }

      return artistViewModel;
    }

    public async Task<CreateNewArtistResponse> CreateNewArtistAsync(
      CreateNewArtistRequest request)
    {
      string artistFullName = string.IsNullOrEmpty(request.LastName) ? request.FirstName : $"{request.FirstName} {request.LastName}";

      string artistSlug = _artistSlugsService.GetArtistSlug(artistFullName);

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
        Slugs = new List<ArtistSlug> { _artistSlugsService.BuildArtistSlug(artistFullName) },
        CreatedAt = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc(),
      };

      _context.Artists.Add(artist);
      await _context.SaveChangesAsync();

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

      await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
      await connection.OpenAsync();

      await using NpgsqlCommand command = new NpgsqlCommand("select a.id, a.first_name, a.last_name, \"as\".name primary_slug, a.has_image from artists a inner join artist_slugs \"as\" on a.id = \"as\".artist_id where a.id in (select distinct artist_id from artist_slugs where name like @artist_name) and a.is_approved = true and a.is_deleted = false and \"as\".is_primary = true;", connection);

      command.Parameters.AddWithValue("@artist_name", $"%{artistNameStandardized}%");

      await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        SearchArtistResultViewModel artist = new SearchArtistResultViewModel();
        int artistId = Convert.ToInt32(reader[0]);
        string name = _textInfo.ToTitleCase(Convert.ToString(reader[1]) + " " + Convert.ToString(reader[2]));
        string primarySlug = Convert.ToString(reader[3]);
        bool hasImage = Convert.ToBoolean(reader[4]);

        string imageUrl = ImageUrlBuilder
          .BuildImageUrl(hasImage, primarySlug, artistId, ImageSize.ExtraSmall);

        string imageAlternateText = ImageUrlBuilder
          .GetImageAlternateText(hasImage, name);

        artist.Name = name;
        artist.PrimarySlug = primarySlug;
        artist.ImageUrl = imageUrl;
        artist.ImageAlternateText = imageAlternateText;

        artists.Add(artist);
      }

      return artists;
    }

    public async Task<IDictionary<char, List<LibraryArtistViewModel>>> GetAllArtistsAsync()
    {
      List<LibraryArtistViewModel> artists = new List<LibraryArtistViewModel>();

      await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
      await connection.OpenAsync();

      await using NpgsqlCommand command = new NpgsqlCommand("select a.id, a.first_name, a.last_name, \"as\".name as primary_slug, a.has_image, count(l.title) as number_of_lyrics from artists a inner join artist_slugs \"as\" on \"as\".artist_id = a.id left join lyrics l on l.artist_id = a.id where a.is_approved = true and a.is_deleted = false and \"as\".is_primary = true and l.is_approved = true and l.is_deleted = false group by a.id, \"as\".name order by a.first_name asc;", connection);

      await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        LibraryArtistViewModel artist = new LibraryArtistViewModel();
        int artistId = Convert.ToInt32(reader[0]);
        string firstName = Convert.ToString(reader[1]);
        string lastName = Convert.ToString(reader[2]);
        string fullName = _textInfo.ToTitleCase($"{firstName} {lastName}").Trim();
        string primarySlug = Convert.ToString(reader[3]);
        bool hasImage = Convert.ToBoolean(reader[4]);
        int numberOfLyrics = Convert.ToInt32(reader[5]);

        string imageUrl = ImageUrlBuilder
          .BuildImageUrl(hasImage, primarySlug, artistId, ImageSize.Small);

        string imageAlternateText = ImageUrlBuilder
          .GetImageAlternateText(hasImage, primarySlug);

        artist.FirstName = firstName;
        artist.LastName = lastName;
        artist.FullName = fullName;
        artist.PrimarySlug = primarySlug;
        artist.ImageUrl = imageUrl;
        artist.ImageAlternateText = imageAlternateText;
        artist.NumberOfLyrics = numberOfLyrics;

        artists.Add(artist);
      }

      IDictionary<char, List<LibraryArtistViewModel>> dictionary = BuildDictionary(artists);

      return dictionary;
    }

    private IDictionary<char, List<LibraryArtistViewModel>> BuildDictionary(List<LibraryArtistViewModel> artists)
    {
      List<char> letters = new List<char>();

      IDictionary<char, List<LibraryArtistViewModel>> dictionary =
        new Dictionary<char, List<LibraryArtistViewModel>>();

      foreach (LibraryArtistViewModel artist in artists)
      {
        char firstLetter = char.ToUpper(artist.FirstName[0]);

        if (!letters.Contains(firstLetter))
        {
          letters.Add(firstLetter);

          dictionary.Add(firstLetter, new List<LibraryArtistViewModel>());
        }
      }

      foreach (char letter in letters)
      {
        foreach (LibraryArtistViewModel artist in artists)
        {
          char firstLetter = char.ToUpper(artist.FirstName[0]);

          if (letter == firstLetter)
          {
            List<LibraryArtistViewModel> artistsBeginningWithTheLetter = dictionary[letter];
            artistsBeginningWithTheLetter.Add(artist);
          }
        }
      }

      return dictionary;
    }
  }
}
