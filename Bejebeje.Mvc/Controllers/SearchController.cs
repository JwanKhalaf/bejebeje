namespace Bejebeje.Mvc.Controllers
{
  using System.Threading.Tasks;
  using Bejebeje.Models.Search;
  using Microsoft.AspNetCore.Mvc;
  using Services.Services.Interfaces;

  public class SearchController : Controller
  {
    private readonly ILyricsService _lyricsService;

    private readonly IArtistsService _artistsService;

    public SearchController(
      ILyricsService lyricsService,
      IArtistsService artistsService)
    {
      _lyricsService = lyricsService;
      _artistsService = artistsService;
    }

    [HttpGet]
    public IActionResult Index()
    {
      SearchViewModel viewModel = new SearchViewModel();

      return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string searchTerm)
    {
      SearchViewModel viewModel = new SearchViewModel();

      if (string.IsNullOrEmpty(searchTerm))
      {
        return View("Index", viewModel);
      }

      viewModel.SearchTerm = searchTerm;

      viewModel.Artists = await _artistsService
        .SearchArtistsAsync(searchTerm);

      viewModel.Lyrics = await _lyricsService
        .SearchLyricsAsync(searchTerm);

      return View("Index", viewModel);
    }
  }
}
