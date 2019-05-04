namespace Bejebeje.Api.Controllers
{
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.AspNetCore.Mvc;

  [Route("[controller]")]
  [ApiController]
  public class ArtistsController : ControllerBase
  {
    private readonly IArtistsService artistsService;

    public ArtistsController(IArtistsService artistService)
    {
      artistsService = artistService;
    }

    [HttpGet]
    public IActionResult Get()
    {
      var artistCards = artistsService.GetArtistCards();
      return Ok(artistCards);
    }
  }
}