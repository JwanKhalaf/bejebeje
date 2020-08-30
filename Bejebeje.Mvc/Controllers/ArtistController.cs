using System.Collections.Generic;
using Bejebeje.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Bejebeje.Models.Lyric;

namespace Bejebeje.Mvc.Controllers
{
  public class ArtistController : Controller
  {
    private readonly ILyricsService lyricsService;

    private readonly ILogger<HomeController> logger;

    public ArtistController(
      ILyricsService lyricsService,
      ILogger<HomeController> logger)
    {
      this.lyricsService = lyricsService;
      this.logger = logger;
    }

    [Route("artists/{artistSlug}/lyrics")]
    public async Task<IActionResult> Index(
      string artistSlug)
    {
      IList<LyricCardViewModel> viewModel = await lyricsService
        .GetLyricsAsync(artistSlug);

      return View(viewModel);
    }
  }
}
