namespace Bejebeje.Services.Services.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.ViewModels.Lyric;

  public interface ILyricsService
  {
    Task<IList<LyricCardViewModel>> GetLyricsByArtistSlugAsync(string artistSlug);
  }
}
