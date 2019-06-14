namespace Bejebeje.Services.Services
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Common.Extensions;
  using Bejebeje.DataAccess.Context;
  using Bejebeje.Services.Services.Interfaces;
  using Bejebeje.Models.Lyric;
  using Microsoft.EntityFrameworkCore;

  public class LyricsService : ILyricsService
  {
    private readonly IArtistsService artistsService;

    private readonly BbContext context;

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
          Slug = l.Slugs.Single(s => s.IsPrimary).Name
        })
        .ToListAsync();

      return lyrics;
    }

    public async Task<IList<LyricCardViewModel>> SearchLyricsAsync(string lyricName)
    {
      string lyricNameStandardized = lyricName.Standardize();

      var test = context.Lyrics.ToList();

      List<LyricCardViewModel> matchedLyrics = await context
        .Lyrics
        .AsNoTracking()
        .Where(x =>
          EF.Functions.Like(x.Title.Standardize(), $"%{lyricNameStandardized}%") ||
          x.Slugs.Any(s => EF.Functions.Like(s.Name.Standardize(), $"%{lyricNameStandardized}%")))
        .Select(x => new LyricCardViewModel
        {
          Title = x.Title,
          Slug = x.Slugs.Single(s => s.IsPrimary).Name
        })
        .ToListAsync();

      return matchedLyrics;
    }

    public async Task<LyricViewModel> GetSingleLyricAsync(string artistSlug, string lyricSlug)
    {
      int artistId = await artistsService.GetArtistIdAsync(artistSlug);

      LyricViewModel lyric = await context
        .Lyrics
        .AsNoTracking()
        .Where(l => l.ArtistId == artistId && l.Slugs.Any(s => s.Name == lyricSlug.Standardize()))
        .Select(l => new LyricViewModel
        {
          Title = l.Title,
          Body = l.Body
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
