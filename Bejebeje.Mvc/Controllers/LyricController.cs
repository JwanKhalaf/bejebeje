using System.Collections.Generic;
using System.Threading.Tasks;
using Bejebeje.Models.Lyric;
using Bejebeje.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bejebeje.Mvc.Controllers
{
  public class LyricController : Controller
  {
    private readonly ILyricsService lyricsService;

    private readonly ILogger<HomeController> logger;

    public LyricController(
      ILyricsService lyricsService,
      ILogger<HomeController> logger)
    {
      this.lyricsService = lyricsService;
      this.logger = logger;
    }

    [Route("artists/{artistSlug}/lyrics")]
    public async Task<IActionResult> ArtistLyrics(
      string artistSlug)
    {
      IList<LyricCardViewModel> viewModel = await lyricsService
        .GetLyricsAsync(artistSlug);

      return View(viewModel);
    }

    [Route("artists/{artistSlug}/lyrics/{lyricSlug}")]
    public async Task<IActionResult> Lyric(
      string artistSlug,
      string lyricSlug)
    {
      LyricViewModel viewModel = await lyricsService
        .GetSingleLyricAsync(artistSlug, lyricSlug);

      return View(viewModel);
    }
  }
}
