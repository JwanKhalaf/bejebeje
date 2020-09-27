namespace Bejebeje.Services.Services
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Threading.Tasks;
  using Bejebeje.Common.Extensions;
  using Common.Enums;
  using Common.Helpers;
  using Config;
  using Extensions;
  using Interfaces;
  using Microsoft.Extensions.Options;
  using Models.Artist;
  using Models.Lyric;
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
      string artistSlug)
    {
      ArtistLyricsViewModel viewModel = new ArtistLyricsViewModel();

      ArtistViewModel artistViewModel = await _artistsService
        .GetArtistDetailsAsync(artistSlug);

      string sqlCommand = "select l.title as lyric_title, lslugs.name as lyric_slug from lyrics as l inner join lyric_slugs as lslugs on lslugs.lyric_id = l.id where artist_id = @artist_id and l.is_deleted = false and l.is_approved = true and lslugs.is_primary = true order by l.title asc;";

      List<LyricCardViewModel> lyricCardViewModels = new List<LyricCardViewModel>();

      await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
      await connection.OpenAsync();

      await using NpgsqlCommand command = new NpgsqlCommand(sqlCommand, connection);

      command.Parameters.AddWithValue("@artist_id", artistViewModel.Id);

      await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        LyricCardViewModel lyricCardViewModel = new LyricCardViewModel();

        string lyricTitle = Convert.ToString(reader[0]);
        string lyricSlug = Convert.ToString(reader[1]);

        lyricCardViewModel.Title = lyricTitle;
        lyricCardViewModel.Slug = lyricSlug;

        lyricCardViewModels.Add(lyricCardViewModel);
      }

      viewModel.Artist = artistViewModel;
      viewModel.Lyrics = lyricCardViewModels;

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
      string lyricSlug)
    {
      LyricDetailsViewModel viewModel = new LyricDetailsViewModel();

      ArtistViewModel artistViewModel = await _artistsService
        .GetArtistDetailsAsync(artistSlug);

      viewModel.Artist = artistViewModel;

      await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
      await connection.OpenAsync();

      await using NpgsqlCommand command = new NpgsqlCommand("select l.title, l.body, l.created_at, l.modified_at from artists as a inner join lyrics as l on l.artist_id = a.id inner join artist_slugs on artist_slugs.artist_id = a.id inner join lyric_slugs as ls on ls.lyric_id = l.id where ls.name = @lyric_slug and artist_slugs.name = @artist_slug;", connection);

      command.Parameters.AddWithValue("@artist_slug", artistSlug);
      command.Parameters.AddWithValue("@lyric_slug", lyricSlug);

      await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        string lyricTitle = Convert.ToString(reader[0]).Trim();
        string lyricBody = Convert.ToString(reader[1]).Trim();
        DateTime lyricCreatedAt = Convert.ToDateTime(reader[2]);
        DateTime? lyricModifiedAt = reader[3] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader[3]);

        viewModel.Title = lyricTitle;
        viewModel.Body = lyricBody;
        viewModel.CreatedAt = lyricCreatedAt;
        viewModel.ModifiedAt = lyricModifiedAt;
      }

      return viewModel;
    }

    public async Task<LyricRecentSubmissionViewModel> GetRecentLyricsAsync()
    {
      LyricRecentSubmissionViewModel lyricRecentSubmissionViewModel = new LyricRecentSubmissionViewModel();
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

      lyricRecentSubmissionViewModel.Lyrics = lyricItemViewModels;

      return lyricRecentSubmissionViewModel;
    }
  }
}
