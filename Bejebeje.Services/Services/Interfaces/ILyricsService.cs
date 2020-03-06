namespace Bejebeje.Services.Services.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.Models.Lyric;

  public interface ILyricsService
  {
    Task<IList<LyricCardViewModel>> GetLyricsAsync(string artistSlug);

    Task<PagedLyricSearchResponse> SearchLyricsAsync(string lyricName, int offset, int limit);

    Task<LyricResponse> GetSingleLyricAsync(string artistSlug, string lyricSlug);
  }
}
