namespace Bejebeje.Mvc.Controllers
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.Models.Artist;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.AspNetCore.Mvc;

  public class ArtistController : Controller
  {
    private readonly IArtistsService _artistsService;

    public ArtistController(
      IArtistsService artistsService)
    {
      _artistsService = artistsService;
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
