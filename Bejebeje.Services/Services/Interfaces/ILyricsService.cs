using System.Collections.Generic;
using System.Threading.Tasks;
using Bejebeje.Models.Lyric;

namespace Bejebeje.Services.Services.Interfaces
{
  public interface ILyricsService
  {
    Task<IList<LyricCardViewModel>> GetLyricsAsync(
      string artistSlug);

    Task<PagedLyricSearchResponse> SearchLyricsAsync(
      string lyricName,
      int offset,
      int limit);

    Task<LyricViewModel> GetSingleLyricAsync(
      string artistSlug,
      string lyricSlug);

    Task<LyricRecentSubmissionViewModel> GetRecentLyricsAsync();
  }
}
