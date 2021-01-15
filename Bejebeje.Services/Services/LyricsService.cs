namespace Bejebeje.Services.Services
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Threading.Tasks;
  using Bejebeje.Common.Extensions;
  using Common.Enums;
  using Common.Exceptions;
  using Common.Helpers;
  using Config;
  using Extensions;
  using Interfaces;
  using Microsoft.Extensions.Options;
  using Models.Artist;
  using Models.Lyric;
  using Models.LyricSlug;
  using Models.Search;
  using Npgsql;

  public class LyricsService : ILyricsService
  {
    private readonly DatabaseOptions _databaseOptions;

    private readonly IArtistsService _artistsService;

    private readonly TextInfo _textInfo = new CultureInfo("ku-TR", false).TextInfo;

    public LyricsService(
      IOptionsMonitor<DatabaseOptions> optionsAccessor,
      IArtistsService artistsService)
    {
      _databaseOptions = optionsAccessor.CurrentValue;
      _artistsService = artistsService;
    }

    public async Task<ArtistLyricsViewModel> GetLyricsAsync(
      string artistSlug,
      string userId)
    {
      ArtistLyricsViewModel viewModel = new ArtistLyricsViewModel();

      ArtistViewModel artistViewModel = await _artistsService
        .GetArtistDetailsAsync(artistSlug, userId);

      if (artistViewModel != null)
      {
        string sqlCommand = "select l.title as lyric_title, lslugs.name as lyric_slug, l.is_approved, l.is_verified from lyrics as l inner join lyric_slugs as lslugs on lslugs.lyric_id = l.id where case when @user_id <> '' then l.user_id = @user_id or l.is_approved = true else l.is_approved = true end and artist_id = @artist_id and l.is_deleted = false and lslugs.is_primary = true order by l.title asc;";

        List<LyricCardViewModel> lyricCardViewModels = new List<LyricCardViewModel>();

        await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
        await connection.OpenAsync();

        await using NpgsqlCommand command = new NpgsqlCommand(sqlCommand, connection);

        command.Parameters.AddWithValue("@user_id", userId);
        command.Parameters.AddWithValue("@artist_id", artistViewModel.Id);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

        if (reader.HasRows)
        {
          while (await reader.ReadAsync())
          {
            LyricCardViewModel lyricCardViewModel = new LyricCardViewModel();

            string title = Convert.ToString(reader[0]);
            string primarySlug = Convert.ToString(reader[1]);
            bool isApproved = Convert.ToBoolean(reader[2]);
            bool isVerified = Convert.ToBoolean(reader[3]);

            lyricCardViewModel.Title = title;
            lyricCardViewModel.Slug = primarySlug;
            lyricCardViewModel.IsAwaitingApproval = !isApproved;
            lyricCardViewModel.IsVerified = isVerified;

            lyricCardViewModels.Add(lyricCardViewModel);
          }

          viewModel.LyricCount = lyricCardViewModels.Count == 1 ? "1 Lyric" : $"{lyricCardViewModels.Count} Lyrics";
          viewModel.Lyrics = lyricCardViewModels;
        }
        else
        {
          viewModel.Lyrics = new List<LyricCardViewModel>();
          viewModel.LyricCount = "0 Lyrics";
        }

        viewModel.Artist = artistViewModel;
      }
      else
      {
        viewModel = null;
      }

      return viewModel;
    }

    public async Task<IEnumerable<SearchLyricResultViewModel>> SearchLyricsAsync(
      string title)
    {
      List<SearchLyricResultViewModel> lyrics = new List<SearchLyricResultViewModel>();

      string lyricTitleStandardized = title.NormalizeStringForUrl();

      await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
      await connection.OpenAsync();

      await using NpgsqlCommand command = new NpgsqlCommand("select l.title as lyric_title, ls.name as lyric_primary_slug, a.first_name artist_first_name, a.last_name artist_last_name, \"as\".name artist_primary_slug from lyrics l inner join lyric_slugs ls on ls.lyric_id = l.id inner join artists a on l.artist_id = a.id inner join artist_slugs \"as\" on a.id = \"as\".artist_id where l.is_deleted = false and l.is_approved = true and \"as\".is_primary = true and ls.name like @lyric_title;", connection);

      command.Parameters.AddWithValue("@lyric_title", $"%{lyricTitleStandardized}%");

      await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        SearchLyricResultViewModel lyric = new SearchLyricResultViewModel();
        string lyricTitle = Convert.ToString(reader[0]);
        string lyricPrimarySlug = Convert.ToString(reader[1]);
        string artistFullName = _textInfo.ToTitleCase(Convert.ToString(reader[2]) + " " + Convert.ToString(reader[3]));
        string artistPrimarySlug = Convert.ToString(reader[4]);

        lyric.Title = lyricTitle;
        lyric.LyricPrimarySlug = lyricPrimarySlug;
        lyric.ArtistFullName = artistFullName;
        lyric.ArtistSlug = artistPrimarySlug;

        lyrics.Add(lyric);
      }

      return lyrics;
    }

    public async Task<LyricDetailsViewModel> GetSingleLyricAsync(
      string artistSlug,
      string lyricSlug,
      string userId)
    {
      LyricDetailsViewModel viewModel = new LyricDetailsViewModel();

      ArtistViewModel artistViewModel = await _artistsService
        .GetArtistDetailsAsync(artistSlug, userId);

      viewModel.Artist = artistViewModel;

      await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
      await connection.OpenAsync();

      await using NpgsqlCommand command = new NpgsqlCommand("select l.id, l.title, l.body, count(likes.lyric_id) as number_of_likes, l.is_verified, l.created_at, l.modified_at, l.is_approved from artists as a inner join lyrics as l on l.artist_id = a.id inner join artist_slugs on artist_slugs.artist_id = a.id inner join lyric_slugs as ls on ls.lyric_id = l.id left join likes on l.id = likes.lyric_id where case when @user_id <> '' then l.user_id = @user_id or a.is_approved = true else l.is_approved = true end and ls.name = @lyric_slug and artist_slugs.name = @artist_slug group by l.id order by number_of_likes;", connection);

      command.Parameters.AddWithValue("@user_id", userId);
      command.Parameters.AddWithValue("@artist_slug", artistSlug);
      command.Parameters.AddWithValue("@lyric_slug", lyricSlug);

      await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        int lyricId = Convert.ToInt32(reader[0]);
        string lyricTitle = Convert.ToString(reader[1]).Trim();
        string lyricBody = Convert.ToString(reader[2]).Trim();
        int numberOfLikes = Convert.ToInt32(reader[3]);
        bool isVerified = Convert.ToBoolean(reader[4]);
        DateTime lyricCreatedAt = Convert.ToDateTime(reader[5]);
        DateTime? lyricModifiedAt = reader[6] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader[6]);
        bool isApproved = Convert.ToBoolean(reader[7]);

        viewModel.Id = lyricId;
        viewModel.Title = lyricTitle;
        viewModel.Body = lyricBody;
        viewModel.NumberOfLikes = numberOfLikes;
        viewModel.IsVerified = isVerified;
        viewModel.CreatedAt = lyricCreatedAt;
        viewModel.ModifiedAt = lyricModifiedAt;
        viewModel.IsApproved = isApproved;
      }

      viewModel.AlreadyLiked = await LyricAlreadyLikedAsync(userId, viewModel.Id);

      return viewModel;
    }

    public async Task<IEnumerable<LyricItemViewModel>> GetRecentLyricsAsync()
    {
      List<LyricItemViewModel> lyricItemViewModels = new List<LyricItemViewModel>();

      await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
      await connection.OpenAsync();

      await using NpgsqlCommand command = new NpgsqlCommand("select a.id, l.title as lyric_title, lslugs.name as primary_lyric_slug, a.first_name, a.last_name, artist_slugs.name as artist_slug, a.has_image from lyrics as l inner join artists as a on l.artist_id = a.id inner join artist_slugs on artist_slugs.artist_id = a.id inner join lyric_slugs as lslugs on lslugs.lyric_id = l.id where artist_slugs.is_primary = true and l.is_approved = true and l.is_deleted = false and lslugs.is_primary = true order by l.created_at desc limit 10;", connection);

      await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        LyricItemViewModel lyricItemViewModel = new LyricItemViewModel();
        int artistId = Convert.ToInt32(reader[0]);
        string lyricTitle = Convert.ToString(reader[1]).Length > 8
          ? Convert.ToString(reader[1]).Truncate(8).Trim() + "…"
          : Convert.ToString(reader[1]);
        string lyricPrimarySlug = Convert.ToString(reader[2]);
        string artistFullName = _textInfo.ToTitleCase(Convert.ToString(reader[3]) + " " + Convert.ToString(reader[4])).Trim();
        string artistPrimarySlug = Convert.ToString(reader[5]);
        bool artistHasImage = Convert.ToBoolean(reader[6]);

        string artistImageUrl = ImageUrlBuilder
          .BuildImageUrl(artistHasImage, artistPrimarySlug, artistId, ImageSize.Small);

        string artistImageAlternateText = ImageUrlBuilder
          .GetImageAlternateText(artistHasImage, artistFullName);

        lyricItemViewModel.ArtistId = artistId;
        lyricItemViewModel.Title = lyricTitle;
        lyricItemViewModel.LyricPrimarySlug = lyricPrimarySlug;
        lyricItemViewModel.ArtistName = artistFullName;
        lyricItemViewModel.ArtistPrimarySlug = artistPrimarySlug;
        lyricItemViewModel.ArtistImageUrl = artistImageUrl;
        lyricItemViewModel.ArtistImageAlternateText = artistImageAlternateText;

        lyricItemViewModels.Add(lyricItemViewModel);
      }

      return lyricItemViewModels;
    }

    public async Task<bool> LyricExistsAsync(
      int lyricId,
      string userId)
    {
      bool lyricExists = false;

      string sqlCommand = "select exists(select 1 from lyrics where case when @user_id <> '' then user_id = @user_id or is_approved = true else is_approved = true end and is_deleted = false and id = @lyric_id);";

      await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);

      await connection.OpenAsync();

      await using NpgsqlCommand command = new NpgsqlCommand(sqlCommand, connection);

      command.Parameters.AddWithValue("@lyric_id", lyricId);
      command.Parameters.AddWithValue("@user_id", userId);

      await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        lyricExists = Convert.ToBoolean(reader[0]);
      }

      return lyricExists;
    }

    public async Task LikeLyricAsync(
      string userId,
      int lyricId)
    {
      bool lyricExists = await LyricExistsAsync(lyricId, userId);

      if (lyricExists)
      {
        string sqlCommand = "insert into likes (user_id, lyric_id) values (@user_id, @lyric_id);";

        await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);

        await connection.OpenAsync();

        await using NpgsqlCommand command = new NpgsqlCommand(sqlCommand, connection);

        command.Parameters.AddWithValue("@user_id", userId);
        command.Parameters.AddWithValue("@lyric_id", lyricId);

        await command.ExecuteNonQueryAsync();
      }
      else
      {
        throw new LyricNotFoundException(lyricId);
      }
    }

    public async Task<bool> LyricAlreadyLikedAsync(
      string userId,
      int lyricId)
    {
      bool lyricAlreadyLiked = false;

      if (string.IsNullOrEmpty(userId))
      {
        return false;
      }

      string sqlCommand = "select exists(select 1 from likes where lyric_id = @lyric_id and user_id = @user_id) as already_liked;";

      await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);

      await connection.OpenAsync();

      await using NpgsqlCommand command = new NpgsqlCommand(sqlCommand, connection);

      command.Parameters.AddWithValue("@user_id", userId);
      command.Parameters.AddWithValue("@lyric_id", lyricId);

      await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        lyricAlreadyLiked = Convert.ToBoolean(reader[0]);
      }

      return lyricAlreadyLiked;
    }

    public async Task<LyricSlugCreateResultViewModel> AddLyricSlugAsync(
      CreateLyricSlugViewModel viewModel)
    {
      LyricSlugCreateResultViewModel result = new LyricSlugCreateResultViewModel();
      string connectionString = _databaseOptions.ConnectionString;
      string sqlStatement = "insert into lyric_slugs (name, is_primary, created_at, is_deleted, lyric_id) values (@name, @is_primary, @created_at, @is_deleted, @lyric_id) returning name";

      string name = viewModel.Name.NormalizeStringForUrl();
      bool isPrimary = true;
      DateTime createdAt = DateTime.UtcNow;
      bool isDeleted = false;
      int lyricId = viewModel.LyricId;

      using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
      {
        NpgsqlCommand command = new NpgsqlCommand(sqlStatement, connection);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@is_primary", isPrimary);
        command.Parameters.AddWithValue("@created_at", createdAt);
        command.Parameters.AddWithValue("@is_deleted", isDeleted);
        command.Parameters.AddWithValue("@lyric_id", lyricId);

        await connection.OpenAsync();

        object artistSlug = command.ExecuteScalar();
        result.LyricPrimarySlug = (string)artistSlug;
      }

      return result;
    }

    public async Task<LyricCreateResultViewModel> AddLyricAsync(
      CreateLyricViewModel viewModel)
    {
      LyricCreateResultViewModel result = new LyricCreateResultViewModel();
      string connectionString = _databaseOptions.ConnectionString;
      string sqlStatement = "insert into lyrics (title, body, user_id, created_at, is_deleted, is_approved, artist_id, is_verified) values (@title, @body, @user_id, @created_at, @is_deleted, @is_approved, @artist_id, @is_verified) returning id;";

      string title = viewModel.Title.ToLower().FirstCharToUpper();
      string body = viewModel.Body;
      string userId = viewModel.UserId;
      DateTime createdAt = DateTime.UtcNow;
      bool isDeleted = false;
      bool isApproved = false;
      int artistId = viewModel.Artist.Id;
      bool isVerified = false;

      using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
      {
        NpgsqlCommand command = new NpgsqlCommand(sqlStatement, connection);
        command.Parameters.AddWithValue("@title", title);
        command.Parameters.AddWithValue("@body", body);
        command.Parameters.AddWithValue("@user_id", userId);
        command.Parameters.AddWithValue("@created_at", createdAt);
        command.Parameters.AddWithValue("@is_deleted", isDeleted);
        command.Parameters.AddWithValue("@is_approved", isApproved);
        command.Parameters.AddWithValue("@artist_id", artistId);
        command.Parameters.AddWithValue("@is_verified", isVerified);

        await connection.OpenAsync();

        object lyricIdentity = command.ExecuteScalar();
        int lyricId = (int)lyricIdentity;

        CreateLyricSlugViewModel createLyricSlugModel = new CreateLyricSlugViewModel();
        createLyricSlugModel.Name = title.NormalizeStringForUrl();
        createLyricSlugModel.LyricId = lyricId;

        LyricSlugCreateResultViewModel slugCreationResult = await AddLyricSlugAsync(createLyricSlugModel);

        result.LyricId = lyricId;
        result.ArtistSlug = viewModel.Artist.PrimarySlug;
        result.LyricSlug = slugCreationResult.LyricPrimarySlug;
      }

      return result;
    }
  }
}
