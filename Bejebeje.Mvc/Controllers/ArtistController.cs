namespace Bejebeje.Mvc.Controllers
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.Models.Artist;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;

  public class ArtistController : Controller
  {
    private readonly IArtistsService _artistsService;

    private readonly ILogger<HomeController> _logger;

    public ArtistController(
      IArtistsService artistsService,
      ILogger<HomeController> logger)
    {
      _artistsService = artistsService;
      _logger = logger;
    }

    [Route("artists")]
    public async Task<IActionResult> Index()
    {
      IDictionary<char, List<LibraryArtistViewModel>> viewModel = await _artistsService
        .GetAllArtistsAsync();

      return View(viewModel);
    }
  }
}
