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
    private readonly IArtistsService _artistsService;

    private readonly ILyricsService _lyricsService;

    public HomeController(
      IArtistsService artistsService,
      ILyricsService lyricsService)
    {
      _artistsService = artistsService;
      _lyricsService = lyricsService;
    }

    public async Task<IActionResult> Index()
    {
      IndexViewModel viewModel = new IndexViewModel();

      viewModel.Lyrics = await _lyricsService
        .GetRecentLyricsAsync();

      viewModel.FemaleArtists = await _artistsService
        .GetTopTenFemaleArtistsByLyricsCountAsync();

      return View(viewModel);
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
