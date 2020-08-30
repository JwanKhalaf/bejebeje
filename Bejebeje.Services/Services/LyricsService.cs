using System;
using Bejebeje.Services.Config;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Bejebeje.Services.Services
{
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Common.Extensions;
  using Bejebeje.DataAccess.Context;
  using Bejebeje.Models.Lyric;
  using Bejebeje.Models.Paging;
  using Bejebeje.Services.Extensions;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.EntityFrameworkCore;

  public class LyricsService : ILyricsService
  {
    private readonly DatabaseOptions _databaseOptions;

    private readonly IArtistsService artistsService;

    private readonly BbContext context;

    private readonly TextInfo textInfo = new CultureInfo("ku-TR", false).TextInfo;

    public LyricsService(
      IOptionsMonitor<DatabaseOptions> optionsAccessor,
      IArtistsService artistsService,
      BbContext context)
    {
      _databaseOptions = optionsAccessor.CurrentValue;
      this.artistsService = artistsService;
      this.context = context;
    }

    public async Task<IList<LyricCardViewModel>> GetLyricsAsync(
      string artistSlug)
    {
      List<LyricCardViewModel> lyricCardViewModels = new List<LyricCardViewModel>();

      await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
      await connection.OpenAsync();

      await using NpgsqlCommand command = new NpgsqlCommand("select l.title from lyrics as l inner join artists as a on l.artist_id = a.id inner join artist_slugs on artist_slugs.artist_id = a.id where artist_slugs.name = @artist_slug order by l.title asc;", connection);

      command.Parameters.AddWithValue("@artist_slug", artistSlug);

      await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        LyricCardViewModel lyricCardViewModel = new LyricCardViewModel();

        string lyricTitle = Convert.ToString(reader[0]);
        string lyricSlug = "slug";

        lyricCardViewModel.Title = lyricTitle;
        lyricCardViewModel.Slug = lyricSlug;

        lyricCardViewModels.Add(lyricCardViewModel);
      }

      return lyricCardViewModels;
    }

    public async Task<PagedLyricSearchResponse> SearchLyricsAsync(string title, int offset, int limit)
    {
      PagedLyricSearchResponse response = new PagedLyricSearchResponse();

      if (string.IsNullOrEmpty(title))
      {
        PagingResponse paging = new PagingResponse();
        paging.Offset = offset;
        paging.Limit = limit;

        response.Paging = paging;

        return response;
      }

      string titleStandardized = title.NormalizeStringForUrl();

      int totalRecords = await context
        .Lyrics
        .AsNoTracking()
        .Where(x =>
          (EF.Functions.Like(x.Title.ToLower(), $"%{titleStandardized}%")
          || x.Slugs.Any(s => EF.Functions.Like(s.Name.ToLower(), $"%{titleStandardized}%")))
          && x.IsApproved
          && !x.IsDeleted)
        .CountAsync();

      List<LyricSearchResponse> matchedLyrics = await context
        .Lyrics
        .Include(l => l.Artist)
        .Include(l => l.Slugs)
        .AsNoTracking()
        .Where(x =>
          (EF.Functions.Like(x.Title.ToLower(), $"%{titleStandardized}%")
          || x.Slugs.Any(s => EF.Functions.Like(s.Name.ToLower(), $"%{titleStandardized}%")))
          && x.IsApproved
          && !x.IsDeleted)
        .OrderBy(l => l.Title)
        .Paging(offset, limit)
        .Select(x => new LyricSearchResponse
        {
          Title = x.Title,
          PrimarySlug = x.Slugs.Single(s => s.IsPrimary).Name,
          Artist = new LyricSearchResponseArtist
          {
            FullName = textInfo.ToTitleCase(x.Artist.FullName),
            PrimarySlug = x.Artist.Slugs.Single(s => s.IsPrimary).Name,
            HasImage = x.Artist.Image != null,
          },
        })
        .ToListAsync();

      response.Lyrics = matchedLyrics;
      response.Paging = new PagingResponse
      {
        Offset = offset,
        Limit = limit,
        Total = totalRecords,
      };

      return response;
    }

    public async Task<LyricViewModel> GetSingleLyricAsync(
      string artistSlug,
      string lyricSlug)
    {
      LyricViewModel lyricViewModel = new LyricViewModel();

      await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
      await connection.OpenAsync();

      await using NpgsqlCommand command = new NpgsqlCommand("select l.title, l.body, l.created_at, l.modified_at from artists as a inner join lyrics as l on l.artist_id = a.id inner join artist_slugs on artist_slugs.artist_id = a.id inner join lyric_slugs as ls on ls.lyric_id = l.id where ls.name = @lyric_slug and artist_slugs.name = @artist_slug;", connection);

      command.Parameters.AddWithValue("@artist_slug", artistSlug);
      command.Parameters.AddWithValue("@lyric_slug", lyricSlug);

      await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        string lyricTitle = Convert.ToString(reader[0]);
        string lyricBody = Convert.ToString(reader[1]);
        DateTime lyricCreatedAt = Convert.ToDateTime(reader[2]);
        DateTime? lyricModifiedAt = reader[3] == System.DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader[3]);

        lyricViewModel.Title = lyricTitle;
        lyricViewModel.Body = lyricBody;
        lyricViewModel.CreatedAt = lyricCreatedAt;
        lyricViewModel.ModifiedAt = lyricModifiedAt;
      }

      return lyricViewModel;
    }

    public async Task<LyricRecentSubmissionViewModel> GetRecentLyricsAsync()
    {
      LyricRecentSubmissionViewModel lyricRecentSubmissionViewModel = new LyricRecentSubmissionViewModel();
      List<LyricItemViewModel> lyricItemViewModels = new List<LyricItemViewModel>();

      await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
      await connection.OpenAsync();

      await using NpgsqlCommand command = new NpgsqlCommand("select l.title, a.first_name, a.last_name, artist_slugs.name from lyrics as l inner join artists as a on l.artist_id = a.id inner join artist_slugs on artist_slugs.artist_id = a.id where artist_slugs.is_primary = true order by l.created_at desc limit 10;", connection);

      await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        LyricItemViewModel lyricItemViewModel = new LyricItemViewModel();
        string lyricTitle = Convert.ToString(reader[0]).Truncate(9);
        string artistFullName = textInfo.ToTitleCase(Convert.ToString(reader[1]) + " " + Convert.ToString(reader[2]));
        string artistPrimarySlug = Convert.ToString(reader[3]);

        lyricItemViewModel.Title = lyricTitle;
        lyricItemViewModel.ArtistName = artistFullName;
        lyricItemViewModel.ArtistPrimarySlug = artistPrimarySlug;

        lyricItemViewModels.Add(lyricItemViewModel);
      }

      lyricRecentSubmissionViewModel.Lyrics = lyricItemViewModels;

      return lyricRecentSubmissionViewModel;
    }
  }
}
