namespace Bejebeje.Services.Services.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.Models.Artist;
  using Bejebeje.Models.Lyric;

  public interface ILyricsService
  {
    Task<ArtistLyricsViewModel> GetLyricsAsync(
      string artistSlug);

    Task<PagedLyricSearchResponse> SearchLyricsAsync(
      string lyricName,
      int offset,
      int limit);

    Task<LyricDetailsViewModel> GetSingleLyricAsync(
      string artistSlug,
      string lyricSlug);

    Task<LyricRecentSubmissionViewModel> GetRecentLyricsAsync();
  }
}
