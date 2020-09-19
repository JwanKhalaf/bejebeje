namespace Bejebeje.Mvc.Controllers
{
  using System.Diagnostics;
  using System.Threading.Tasks;
  using Bejebeje.Models.Lyric;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.AspNetCore.Mvc;
  using Models;

  public class HomeController : Controller
  {
    private readonly ILyricsService _lyricsService;

    public HomeController(
      ILyricsService lyricsService)
    {
      _lyricsService = lyricsService;
    }

    public async Task<IActionResult> Index()
    {
      LyricRecentSubmissionViewModel lyricRecentSubmissionViewModel = await _lyricsService
        .GetRecentLyricsAsync();

      return View(lyricRecentSubmissionViewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      ErrorViewModel viewModel = new ErrorViewModel
      {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
      };

      return View(viewModel);
    }
  }
}
