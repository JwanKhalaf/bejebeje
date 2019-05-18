namespace Bejebeje.Services.Services
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Common.Extensions;
  using Bejebeje.DataAccess.Context;
  using Bejebeje.Services.Services.Interfaces;
  using Bejebeje.ViewModels.Lyric;
  using Microsoft.EntityFrameworkCore;

  public class LyricsService : ILyricsService
  {
    private readonly BbContext context;

    public LyricsService(BbContext context)
    {
      this.context = context;
    }

    public async Task<IList<LyricCardViewModel>> GetLyricsAsync(string artistSlug)
    {
      int artistId = await GetArtistIdAsync(artistSlug);

      if (artistId == 0)
      {
        throw new ArtistNotFoundException(artistSlug);
      }

      List<LyricCardViewModel> lyrics = await context
        .Lyrics
        .AsNoTracking()
        .Where(l => l.ArtistId == artistId)
        .Select(l => new LyricCardViewModel
        {
          Title = l.Title,
          Slug = l.Slugs.Where(s => s.IsPrimary).Select(s => s.Name).Single()
        })
        .ToListAsync();

      return lyrics;
    }

    public async Task<LyricViewModel> GetSingleLyricAsync(string artistSlug, string lyricSlug)
    {
      int artistId = await GetArtistIdAsync(artistSlug);

      if (artistId == 0)
      {
        throw new ArtistNotFoundException(artistSlug);
      }

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

    private async Task<int> GetArtistIdAsync(string artistSlug)
    {
      int artistId = await context
        .Artists
        .AsNoTracking()
        .Where(x => x.Slugs.Any(y => y.Name == artistSlug.Standardize()))
        .Select(x => x.Id)
        .FirstOrDefaultAsync();

      return artistId;
    }
  }
}
