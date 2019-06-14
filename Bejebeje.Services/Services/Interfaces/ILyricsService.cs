namespace Bejebeje.Services.Services.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.Models.Lyric;

  public interface ILyricsService
  {
    Task<IList<LyricCardViewModel>> GetLyricsAsync(string artistSlug);

    Task<IList<LyricCardViewModel>> SearchLyricsAsync(string lyricName);

    Task<LyricViewModel> GetSingleLyricAsync(string artistSlug, string lyricSlug);
  }
}
