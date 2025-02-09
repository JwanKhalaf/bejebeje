namespace Bejebeje.Services.Services;

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
using Models.ArtistSlug;
using Models.Search;
using NodaTime;
using Npgsql;

public class ArtistsService : IArtistsService
{
  private readonly DatabaseOptions _databaseOptions;

  private readonly IArtistSlugsService _artistSlugsService;

  private readonly ICognitoService _cognitoService;

  private readonly IEmailService _emailService;

  private readonly BbContext _context;

  private readonly TextInfo _textInfo = new CultureInfo("ku-TR", false).TextInfo;

  public ArtistsService(
    IOptionsMonitor<DatabaseOptions> optionsAccessor,
    IArtistSlugsService artistSlugsService,
    ICognitoService cognitoService,
    IEmailService emailService,
    BbContext context)
  {
    _databaseOptions = optionsAccessor.CurrentValue;
    _artistSlugsService = artistSlugsService;
    _cognitoService = cognitoService;
    _emailService = emailService;
    _context = context;
  }

  public async Task<int> GetArtistIdAsync(string artistSlug)
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

  public async Task<ArtistViewModel> GetArtistDetailsByIdAsync(int artistId, string userId)
  {
    ArtistViewModel artistViewModel = new ArtistViewModel();

    await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
    await connection.OpenAsync();

    string sqlCommand =
      @"select a.first_name, a.last_name, aslug.name as artist_slug, a.has_image, a.created_at, a.modified_at, a.is_approved, a.user_id from artists as a inner join artist_slugs as aslug on aslug.artist_id = a.id where a.id = @artist_id and aslug.is_primary = true;";

    await using NpgsqlCommand command = new NpgsqlCommand(sqlCommand, connection);

    command.Parameters.AddWithValue("@artist_id", artistId);

    await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

    if (reader.HasRows)
    {
      while (await reader.ReadAsync())
      {
        string firstName = _textInfo.ToTitleCase(Convert.ToString(reader[0])).Trim();
        string lastName = _textInfo.ToTitleCase(Convert.ToString(reader[1])).Trim();
        string artistPrimarySlug = Convert.ToString(reader[2]);
        bool artistHasImage = Convert.ToBoolean(reader[3]);
        DateTime createdAt = Convert.ToDateTime(reader[4]);
        DateTime? modifiedAt = reader[5] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader[5]);
        bool isApproved = Convert.ToBoolean(reader[6]);
        string submitterUserId = Convert.ToString(reader[7]);

        string artistFullName = $"{firstName} {lastName}".Trim();

        string artistImageUrl = ImageUrlBuilder
          .BuildImageUrl(artistHasImage, artistId, ImageSize.Standard);

        string artistImageAlternateText = ImageUrlBuilder
          .GetImageAlternateText(artistHasImage, artistFullName);

        artistViewModel.Id = artistId;
        artistViewModel.FirstName = firstName;
        artistViewModel.LastName = lastName;
        artistViewModel.FullName = $"{firstName} {lastName}";
        artistViewModel.PrimarySlug = artistPrimarySlug;
        artistViewModel.IsApproved = isApproved;
        artistViewModel.ImageUrl = artistImageUrl;
        artistViewModel.ImageAlternateText = artistImageAlternateText;
        artistViewModel.CreatedAt = createdAt;
        artistViewModel.ModifiedAt = modifiedAt;
        artistViewModel.IsOwnSubmission = submitterUserId == userId;
      }
    }
    else
    {
      throw new ArtistNotFoundException(artistId);
    }

    return artistViewModel;
  }

  public async Task<bool> ArtistExistsAsync(string artistSlug)
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

  public async Task<IEnumerable<RandomFemaleArtistItemViewModel>> GetTopTenFemaleArtistsByLyricsCountAsync()
  {
    List<RandomFemaleArtistItemViewModel> femaleArtists = new List<RandomFemaleArtistItemViewModel>();

    await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
    await connection.OpenAsync();

    string sqlCommand =
      @"select a.id, a.first_name, a.last_name, s.""name"", a.has_image, count(l.id) as number_of_lyrics from artists a left join lyrics l on a.id = l.artist_id inner join artist_slugs s on s.artist_id = a.id where a.sex = 'f' and s.is_primary = true group by a.id, s.""name"" order by number_of_lyrics desc limit 10;";

    await using NpgsqlCommand command = new NpgsqlCommand(sqlCommand, connection);

    await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
      int id = Convert.ToInt32(reader[0]);
      string name = _textInfo.ToTitleCase(Convert.ToString(reader[1]) + " " + Convert.ToString(reader[2])).Trim();
      string primarySlug = Convert.ToString(reader[3]);
      bool hasImage = Convert.ToBoolean(reader[4]);

      string artistImageUrl = ImageUrlBuilder
        .BuildImageUrl(hasImage, id, ImageSize.Small);

      string artistImageAlternateText = ImageUrlBuilder
        .GetImageAlternateText(hasImage, name);

      RandomFemaleArtistItemViewModel artist = new RandomFemaleArtistItemViewModel();

      artist.Name = name.TruncateLongString(9);
      artist.PrimarySlug = primarySlug;
      artist.ImageUrl = artistImageUrl;
      artist.ImageAlternateText = artistImageAlternateText;

      femaleArtists.Add(artist);
    }

    return femaleArtists;
  }

  public async Task<ArtistViewModel> GetArtistDetailsAsync(string artistSlug, string userId)
  {
    ArtistViewModel artistViewModel = new ArtistViewModel();

    await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
    await connection.OpenAsync();

    string sqlCommand =
      @"select a.id as artist_id, a.first_name, a.last_name, aslug.name as artist_slug, a.has_image, a.created_at, a.modified_at, a.is_approved, a.user_id from artists as a inner join artist_slugs as aslug on aslug.artist_id = a.id where case when @user_id <> '' then (a.is_approved = true or a.user_id = @user_id) else a.is_approved = true end and aslug.artist_id = (select artist_id from artist_slugs where name = @artist_slug) and aslug.is_primary = true and a.is_deleted = false order by a.first_name asc;";

    await using NpgsqlCommand command = new NpgsqlCommand(sqlCommand, connection);

    command.Parameters.AddWithValue("@user_id", userId);
    command.Parameters.AddWithValue("@artist_slug", artistSlug);

    await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

    if (reader.HasRows)
    {
      while (await reader.ReadAsync())
      {
        int artistId = Convert.ToInt32(reader[0]);
        string firstName = _textInfo.ToTitleCase(Convert.ToString(reader[1])).Trim();
        string lastName = _textInfo.ToTitleCase(Convert.ToString(reader[2])).Trim();
        string artistPrimarySlug = Convert.ToString(reader[3]);
        bool artistHasImage = Convert.ToBoolean(reader[4]);
        DateTime createdAt = Convert.ToDateTime(reader[5]);
        DateTime? modifiedAt = reader[6] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader[6]);
        bool isApproved = Convert.ToBoolean(reader[7]);
        string submitterUserId = Convert.ToString(reader[8]);

        string artistFullName = $"{firstName} {lastName}".Trim();

        string artistImageUrl = ImageUrlBuilder
          .BuildImageUrl(artistHasImage, artistId, ImageSize.Standard);

        string artistImageAlternateText = ImageUrlBuilder
          .GetImageAlternateText(artistHasImage, artistFullName);

        artistViewModel.Id = artistId;
        artistViewModel.FirstName = firstName;
        artistViewModel.LastName = lastName;
        artistViewModel.FullName = $"{firstName} {lastName}";
        artistViewModel.PrimarySlug = artistPrimarySlug;
        artistViewModel.IsApproved = isApproved;
        artistViewModel.ImageUrl = artistImageUrl;
        artistViewModel.ImageAlternateText = artistImageAlternateText;
        artistViewModel.CreatedAt = createdAt;
        artistViewModel.ModifiedAt = modifiedAt;
        artistViewModel.IsOwnSubmission = submitterUserId == userId;
      }
    }
    else
    {
      throw new ArtistNotFoundException(artistSlug);
    }

    return artistViewModel;
  }

  public async Task<CreateNewArtistResponse> CreateNewArtistAsync(CreateNewArtistRequest request)
  {
    string artistFullName = string.IsNullOrEmpty(request.LastName)
      ? request.FirstName
      : $"{request.FirstName} {request.LastName}";

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

  public async Task<IEnumerable<SearchArtistResultViewModel>> SearchArtistsAsync(string artistName)
  {
    string artistNameStandardized = artistName.NormalizeStringForUrl();

    List<SearchArtistResultViewModel> artists = new List<SearchArtistResultViewModel>();

    await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
    await connection.OpenAsync();

    await using NpgsqlCommand command = new NpgsqlCommand(
      "select a.id, a.first_name, a.last_name, \"as\".name primary_slug, a.has_image from artists a inner join artist_slugs \"as\" on a.id = \"as\".artist_id where a.id in (select distinct artist_id from artist_slugs where name like @artist_name) and a.is_approved = true and a.is_deleted = false and \"as\".is_primary = true;",
      connection);

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
        .BuildImageUrl(hasImage, artistId, ImageSize.ExtraSmall);

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

    await using NpgsqlCommand command = new NpgsqlCommand(
      "select a.id, a.first_name, a.last_name, \"as\".name as primary_slug, a.has_image, count(l.title) as number_of_lyrics from artists a inner join artist_slugs \"as\" on \"as\".artist_id = a.id left join lyrics l on l.artist_id = a.id where a.is_approved = true and a.is_deleted = false and \"as\".is_primary = true and l.is_approved = true and l.is_deleted = false group by a.id, \"as\".name order by a.first_name asc;",
      connection);

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
        .BuildImageUrl(hasImage, artistId, ImageSize.Small);

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

  public async Task<ArtistCreationResult> AddArtistAsync(CreateArtistViewModel viewModel)
  {
    ArtistCreationResult result = new ArtistCreationResult();
    string connectionString = _databaseOptions.ConnectionString;
    string sqlStatement =
      "insert into artists (first_name, last_name, full_name, is_approved, user_id, created_at, is_deleted, has_image) values (@first_name, @last_name, @full_name, @is_approved, @user_id, @created_at, @is_deleted, @has_image) returning id";
    int artistId = 0;

    string firstName = viewModel.FirstName.ToLower();
    string lastName = viewModel.LastName.ToLower();
    string fullName = string.IsNullOrEmpty(lastName) ? firstName : $"{firstName} {lastName}";
    bool isApproved = false;
    DateTime createdAt = DateTime.UtcNow;
    string userId = viewModel.UserId;
    bool isDeleted = false;
    bool hasImage = false;

    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
    {
      NpgsqlCommand command = new NpgsqlCommand(sqlStatement, connection);
      command.Parameters.AddWithValue("@first_name", firstName);
      command.Parameters.AddWithValue("@last_name", lastName);
      command.Parameters.AddWithValue("@full_name", fullName);
      command.Parameters.AddWithValue("@is_approved", isApproved);
      command.Parameters.AddWithValue("@user_id", userId);
      command.Parameters.AddWithValue("@created_at", createdAt);
      command.Parameters.AddWithValue("@is_deleted", isDeleted);
      command.Parameters.AddWithValue("@has_image", hasImage);

      try
      {
        await connection.OpenAsync();

        object artistIdentity = command.ExecuteScalar();
        artistId = (int)artistIdentity;

        ArtistSlugCreateViewModel artistSlug = new ArtistSlugCreateViewModel();
        artistSlug.Name = fullName.NormalizeStringForUrl();
        artistSlug.IsPrimary = true;
        artistSlug.CreatedAt = createdAt;
        artistSlug.ArtistId = artistId;

        await _artistSlugsService.AddArtistSlugAsync(artistSlug);

        result.PrimarySlug = artistSlug.Name;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }

    string submitterName = await _cognitoService.GetPreferredUsernameAsync(userId);

    await _emailService.SendArtistSubmissionEmailAsync(submitterName, _textInfo.ToTitleCase(fullName).Trim());

    return result;
  }

  public async Task<ArtistUpdateResult> UpdateArtistAsync(UpdateArtistViewModel viewModel)
  {
    ArtistUpdateResult result = new ArtistUpdateResult();
    string connectionString = _databaseOptions.ConnectionString;
    string sqlStatement =
      "update artists set first_name = @first_name, last_name = @last_name, full_name = @full_name, modified_at = current_timestamp where id = @artist_id";

    int artistId = viewModel.Id;
    string firstName = viewModel.FirstName.ToLower();
    string lastName = viewModel.LastName.ToLower();
    string fullName = string.IsNullOrEmpty(lastName) ? firstName : $"{firstName} {lastName}";
    DateTime timestamp = DateTime.UtcNow;

    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
    {
      NpgsqlCommand command = new NpgsqlCommand(sqlStatement, connection);
      command.Parameters.AddWithValue("@first_name", firstName);
      command.Parameters.AddWithValue("@last_name", lastName);
      command.Parameters.AddWithValue("@full_name", fullName);
      command.Parameters.AddWithValue("@artist_id", artistId);

      try
      {
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();

        ArtistSlugCreateViewModel artistSlug = new ArtistSlugCreateViewModel
        {
          Name = fullName.NormalizeStringForUrl(),
          IsPrimary = true,
          CreatedAt = timestamp,
          ArtistId = artistId,
        };

        await _artistSlugsService.AddArtistSlugAsync(artistSlug);

        result.PrimarySlug = artistSlug.Name;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }

    return result;
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