using System.Diagnostics;
using System.Threading.Tasks;
using Bejebeje.Models.Lyric;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Bejebeje.Mvc.Models;
using Bejebeje.Services.Services.Interfaces;

namespace Bejebeje.Mvc.Controllers
{
  public class HomeController : Controller
  {
    private readonly ILyricsService lyricsService;

    private readonly ILogger<HomeController> logger;

    public HomeController(
      ILyricsService lyricsService,
      ILogger<HomeController> logger)
    {
      this.lyricsService = lyricsService;
      this.logger = logger;
    }

    public async Task<IActionResult> Index()
    {
      LyricRecentSubmissionViewModel lyricRecentSubmissionViewModel = await lyricsService
        .GetRecentLyricsAsync();
      
      return View(lyricRecentSubmissionViewModel);
    }

    public IActionResult Privacy()
    {
      return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      ErrorViewModel viewModel = new ErrorViewModel
      {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
      };

      return View(viewModel);
    }
  }
}
