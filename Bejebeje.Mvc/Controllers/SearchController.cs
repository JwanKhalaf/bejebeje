namespace Bejebeje.Mvc.Controllers
{
  using System.Threading.Tasks;
  using Bejebeje.Models.Search;
  using Microsoft.AspNetCore.Mvc;
  using Services.Services.Interfaces;
  using System.Linq;

  public class SearchController : Controller
  {
    private readonly ILyricsService lyricsService;

    private readonly IArtistsService artistsService;

    public SearchController(
      ILyricsService lyricsService,
      IArtistsService artistsService)
    {
      this.lyricsService = lyricsService;
      this.artistsService = artistsService;
    }

    [HttpGet]
    public IActionResult Index()
    {
      SearchViewModel viewModel = new SearchViewModel();

      return this.View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Index(string searchTerm)
    {
      SearchViewModel viewModel = new SearchViewModel();

      if (string.IsNullOrEmpty(searchTerm))
      {
        return View(viewModel);
      }

      viewModel.SearchTerm = searchTerm;

      viewModel.Artists = await this.artistsService
        .SearchArtistsAsync(searchTerm);

      viewModel.Lyrics = await this.lyricsService
        .SearchLyricsAsync(searchTerm);

      return this.View(viewModel);
    }
  }
}
