using Bejebeje.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bejebeje.Api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ArtistsController : ControllerBase
  {
    private readonly IArtistsService artistsService;

    public ArtistsController(IArtistsService artistService)
    {
      this.artistsService = artistService;
    }

    [HttpGet]
    public IActionResult Get()
    {
      var artistCards = artistsService.GetArtistCards();
      return Ok(artistCards);
    }
  }
}