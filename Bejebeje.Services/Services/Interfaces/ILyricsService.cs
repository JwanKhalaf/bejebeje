namespace Bejebeje.Services.Services.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Models.Artist;
  using Models.Lyric;
  using Models.LyricSlug;
  using Models.Search;

  public interface ILyricsService
  {
    Task<ArtistLyricsViewModel> GetLyricsAsync(
      string artistSlug,
      string userId);

    Task<IEnumerable<SearchLyricResultViewModel>> SearchLyricsAsync(
      string title);

    Task<LyricDetailsViewModel> GetSingleLyricAsync(
      string artistSlug,
      string lyricSlug,
      string userId);

    Task<IEnumerable<LyricItemViewModel>> GetRecentlySubmittedLyricsAsync();

    Task<IEnumerable<LyricItemViewModel>> GetRecentlyVerifiedLyricsAsync();

    Task<bool> LyricExistsAsync(
      int lyricId,
      string userId);

    Task LikeLyricAsync(
      string userId,
      int lyricId);

    Task<bool> LyricAlreadyLikedAsync(
      string userId,
      int lyricId);

    Task<LyricSlugCreateResultViewModel> AddLyricSlugAsync(
      CreateLyricSlugViewModel viewModel);

    Task<LyricCreateResultViewModel> AddLyricAsync(
      CreateLyricViewModel viewModel);
  }
}