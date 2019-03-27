using Bejebeje.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bejebeje.Api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ArtistController : ControllerBase
  {
    private readonly IArtistService artistService;

    public ArtistController(IArtistService artistService)
    {
      this.artistService = artistService;
    }

    [HttpGet]
    public IActionResult Get()
    {
      var artistCards = artistService.GetArtistCards();
      return Ok(artistCards);
    }
  }
}