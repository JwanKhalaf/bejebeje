namespace Bejebeje.Services.Services.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.ViewModels.Lyric;

  public interface ILyricsService
  {
    Task<IList<LyricCardViewModel>> GetLyricsAsync(string artistSlug);

    Task<LyricViewModel> GetLyricAsync(string artistSlug, string lyricSlug);
  }
}
