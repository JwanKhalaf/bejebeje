namespace Bejebeje.Services.Services.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Models.Artist;
  using Models.Lyric;
  using Models.Search;

  public interface ILyricsService
  {
    Task<ArtistLyricsViewModel> GetLyricsAsync(
      string artistSlug);

    Task<IEnumerable<SearchLyricResultViewModel>> SearchLyricsAsync(
      string title);

    Task<LyricDetailsViewModel> GetSingleLyricAsync(
      string artistSlug,
      string lyricSlug,
      string userId);

    Task<IEnumerable<LyricItemViewModel>> GetRecentLyricsAsync();

    Task<bool> LyricExistsAsync(
      int lyricId);

    Task LikeLyricAsync(
      string userId,
      int lyricId);

    Task<bool> LyricAlreadyLikedAsync(
      string userId,
      int lyricId);
  }
}
