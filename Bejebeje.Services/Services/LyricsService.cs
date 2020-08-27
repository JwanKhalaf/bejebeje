using System;
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
    private readonly IArtistsService artistsService;

    private readonly BbContext context;

    private readonly TextInfo textInfo = new CultureInfo("ku-TR", false).TextInfo;

    public LyricsService(
      IArtistsService artistsService,
      BbContext context)
    {
      this.artistsService = artistsService;
      this.context = context;
    }

    public async Task<IList<LyricCardViewModel>> GetLyricsAsync(string artistSlug)
    {
      int artistId = await artistsService.GetArtistIdAsync(artistSlug);

      List<LyricCardViewModel> lyrics = await context
        .Lyrics
        .AsNoTracking()
        .Where(l => l.ArtistId == artistId && l.IsApproved && !l.IsDeleted)
        .OrderBy(l => l.Title)
        .Select(l => new LyricCardViewModel
        {
          Title = l.Title,
          Slug = l.Slugs.Single(s => s.IsPrimary).Name,
        })
        .ToListAsync();

      return lyrics;
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

    public async Task<LyricResponse> GetSingleLyricAsync(string artistSlug, string lyricSlug)
    {
      int artistId = await artistsService.GetArtistIdAsync(artistSlug);

      LyricResponse lyric = await context
        .Lyrics
        .AsNoTracking()
        .Where(l => l.ArtistId == artistId && l.Slugs.Any(s => s.Name == lyricSlug.Standardize()))
        .Select(l => new LyricResponse
        {
          Title = l.Title,
          Body = l.Body,
          AuthorSlug = l.Author != null ? l.Author.Slugs.Where(s => s.IsPrimary).SingleOrDefault().Name : string.Empty,
          CreatedAt = l.CreatedAt,
          ModifiedAt = l.ModifiedAt,
        })
        .SingleOrDefaultAsync();

      if (lyric == null)
      {
        throw new LyricNotFoundException(artistSlug, lyricSlug);
      }

      return lyric;
    }

    public async Task<LyricRecentSubmissionViewModel> GetRecentLyricsAsync()
    {
      LyricRecentSubmissionViewModel lyricRecentSubmissionViewModel = new LyricRecentSubmissionViewModel();
      List<LyricItemViewModel> lyricItemViewModels = new List<LyricItemViewModel>();

      string connectionString = "Server=localhost;Port=5432;Database=bejebeje;User Id=bejebeje_user;Password=admin;";

      await using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
      await connection.OpenAsync();

      await using NpgsqlCommand command = new NpgsqlCommand("select l.title, a.first_name, a.last_name from lyrics as l inner join artists as a on l.artist_id = a.id order by l.created_at desc limit 10;", connection);

      await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        LyricItemViewModel lyricItemViewModel = new LyricItemViewModel();
        string lyricTitle = Convert.ToString(reader[0]).Truncate(9);
        string artistFullName = textInfo.ToTitleCase(Convert.ToString(reader[1]) + " " + Convert.ToString(reader[2]));

        lyricItemViewModel.Title = lyricTitle;
        lyricItemViewModel.ArtistName = artistFullName;

        lyricItemViewModels.Add(lyricItemViewModel);
      }

      lyricRecentSubmissionViewModel.Lyrics = lyricItemViewModels;

      return lyricRecentSubmissionViewModel;
    }
  }
}
