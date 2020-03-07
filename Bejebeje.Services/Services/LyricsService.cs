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

    private TextInfo textInfo = new CultureInfo("ku-TR", false).TextInfo;

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
        .Where(l => l.ArtistId == artistId)
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
      string titleStandardized = title.Standardize();

      int totalRecords = await context
        .Lyrics
        .AsNoTracking()
        .Where(x => EF.Functions.Like(x.Title.ToLower(), $"%{titleStandardized}%") || x.Slugs.Any(s => EF.Functions.Like(s.Name.ToLower(), $"%{titleStandardized}%")))
        .CountAsync();

      var test = await context
        .Lyrics
        .Include(l => l.Artist)
        .Include(l => l.Slugs)
        .AsNoTracking()
        .Where(x => EF.Functions.Like(x.Title.ToLower(), $"%{titleStandardized}%") || x.Slugs.Any(s => EF.Functions.Like(s.Name.ToLower(), $"%{titleStandardized}%")))
        .Paging(offset, limit)
        .ToListAsync();

      List<LyricSearchResponse> matchedLyrics = await context
        .Lyrics
        .Include(l => l.Artist)
        .Include(l => l.Slugs)
        .AsNoTracking()
        .Where(x => EF.Functions.Like(x.Title.ToLower(), $"%{titleStandardized}%") || x.Slugs.Any(s => EF.Functions.Like(s.Name.ToLower(), $"%{titleStandardized}%")))
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

      PagedLyricSearchResponse response = new PagedLyricSearchResponse
      {
        Lyrics = matchedLyrics,
        Paging = new PagingResponse
        {
          Offset = offset,
          Limit = limit,
          Total = totalRecords,
        },
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
  }
}
