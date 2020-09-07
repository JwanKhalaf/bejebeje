namespace Bejebeje.Mvc.Controllers
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Bejebeje.Models.Lyric;
    using Bejebeje.Mvc.Models;
    using Bejebeje.Services.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

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
      LyricRecentSubmissionViewModel lyricRecentSubmissionViewModel = await this.lyricsService
        .GetRecentLyricsAsync();
      
      return this.View(lyricRecentSubmissionViewModel);
    }

    public IActionResult Privacy()
    {
      return this.View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      ErrorViewModel viewModel = new ErrorViewModel
      {
        RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier
      };

      return this.View(viewModel);
    }
  }
}
