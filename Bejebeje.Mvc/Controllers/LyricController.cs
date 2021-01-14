namespace Bejebeje.Mvc.Controllers
{
  using System.Threading.Tasks;
  using Bejebeje.Models.Artist;
  using Bejebeje.Models.Lyric;
  using Bejebeje.Services.Services.Interfaces;
  using Extensions;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  public class LyricController : Controller
  {
    private readonly IArtistsService _artistsService;

    private readonly ILyricsService _lyricsService;

    public LyricController(
      IArtistsService artistsService,
      ILyricsService lyricsService)
    {
      _artistsService = artistsService;
      _lyricsService = lyricsService;
    }

    [Route("artists/{artistSlug}/lyrics")]
    public async Task<IActionResult> ArtistLyrics(
      string artistSlug)
    {
      string userId = User.Identity.IsAuthenticated
        ? User.GetUserId().ToString()
        : string.Empty;

      ArtistLyricsViewModel viewModel = await _lyricsService
        .GetLyricsAsync(artistSlug, userId);

      if (viewModel == null)
      {
        return RedirectToAction("Index", "Home");
      }

      return View(viewModel);
    }

    [Route("artists/{artistSlug}/lyrics/{lyricSlug}")]
    public async Task<IActionResult> Lyric(
      string artistSlug,
      string lyricSlug)
    {
      string userId = User.Identity.IsAuthenticated
        ? User.GetUserId().ToString()
        : string.Empty;

      LyricDetailsViewModel viewModel = await _lyricsService
        .GetSingleLyricAsync(artistSlug, lyricSlug, userId);

      viewModel.PrimarySlug = lyricSlug;

      return View(viewModel);
    }

    [Authorize]
    [Route("lyrics/like/{lyricId}")]
    public async Task<IActionResult> Like(
      string artistSlug,
      string lyricSlug,
      int lyricId)
    {
      try
      {
        string userId = User.GetUserId().ToString();

        await _lyricsService.LikeLyricAsync(userId, lyricId);

        return RedirectToAction("Lyric", new { artistSlug = artistSlug, lyricSlug = lyricSlug });
      }
      catch
      {
        return RedirectToAction("Lyric", new { artistSlug = artistSlug, lyricSlug = lyricSlug });
      }
    }
  }
}
