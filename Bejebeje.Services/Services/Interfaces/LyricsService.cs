namespace Bejebeje.Services.Services.Interfaces
{
  using System.Collections.Generic;
  using System.Linq;
  using Bejebeje.Common.Extensions;
  using Bejebeje.DataAccess.Context;
  using Bejebeje.ViewModels.Lyric;
  using Microsoft.EntityFrameworkCore;

  public class LyricsService : ILyricsService
  {
    private readonly BbContext context;

    public LyricsService(BbContext context)
    {
      this.context = context;
    }

    public IList<LyricCardViewModel> GetLyricsByArtistSlug(string artistSlug)
    {
      int artistId = context
        .Artists
        .AsNoTracking()
        .Where(x => x.Slugs.Any(y => y.Name == artistSlug.Standardize()))
        .Select(x => x.Id)
        .Single();

      List<LyricCardViewModel> lyrics = context
        .Lyrics
        .AsNoTracking()
        .Where(l => l.ArtistId == artistId)
        .Select(l => new LyricCardViewModel
        {
          Title = l.Title,
          Slug = l.Slugs.Where(s => s.IsPrimary).Select(s => s.Name).Single()
        })
        .ToList();

      return lyrics;
    }
  }
}
