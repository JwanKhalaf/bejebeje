namespace Bejebeje.Services.Services.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.Models.Artist;
  using Bejebeje.Models.Lyric;
  using Models.Search;

  public interface ILyricsService
  {
    Task<ArtistLyricsViewModel> GetLyricsAsync(
      string artistSlug);

    Task<IEnumerable<SearchLyricResultViewModel>> SearchLyricsAsync(
      string title);

    Task<LyricDetailsViewModel> GetSingleLyricAsync(
      string artistSlug,
      string lyricSlug);

    Task<LyricRecentSubmissionViewModel> GetRecentLyricsAsync();
  }
}
