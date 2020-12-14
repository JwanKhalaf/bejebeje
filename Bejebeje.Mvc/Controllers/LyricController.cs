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
    private readonly ILyricsService _lyricsService;

    public LyricController(
      ILyricsService lyricsService)
    {
      _lyricsService = lyricsService;
    }

    [Route("artists/{artistSlug}/lyrics")]
    public async Task<IActionResult> ArtistLyrics(
      string artistSlug)
    {
      ArtistLyricsViewModel viewModel = await _lyricsService
        .GetLyricsAsync(artistSlug);

      return View(viewModel);
    }

    [Route("artists/{artistSlug}/lyrics/{lyricSlug}")]
    public async Task<IActionResult> Lyric(
      string artistSlug,
      string lyricSlug)
    {
      string userId = User.Identity.IsAuthenticated ? User.GetUserId().ToString() : string.Empty;

      LyricDetailsViewModel viewModel = await _lyricsService
        .GetSingleLyricAsync(artistSlug, lyricSlug, userId);

      return View(viewModel);
    }

    [Authorize]
    [Route("lyrics/like/{lyricId}")]
    public async Task<IActionResult> Like(
      int lyricId)
    {
      try
      {
        string userId = User.GetUserId().ToString();

        await _lyricsService.LikeLyricAsync(userId, lyricId);

        return RedirectToAction("Index", "Home");
      }
      catch
      {
        return RedirectToAction("Index", "Home");
      }
    }
  }
}
