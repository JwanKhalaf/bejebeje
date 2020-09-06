namespace Bejebeje.Mvc.Controllers
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.Models.Artist;
  using Bejebeje.Models.Lyric;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;

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
      ArtistLyricsViewModel viewModel = await this.lyricsService
        .GetLyricsAsync(artistSlug);

      return View(viewModel);
    }

    [Route("artists/{artistSlug}/lyrics/{lyricSlug}")]
    public async Task<IActionResult> Lyric(
      string artistSlug,
      string lyricSlug)
    {
      LyricDetailsViewModel viewModel = await this.lyricsService
        .GetSingleLyricAsync(artistSlug, lyricSlug);

      return this.View(viewModel);
    }
  }
}
