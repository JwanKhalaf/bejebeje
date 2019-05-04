namespace Bejebeje.Api.Controllers
{
  using System.Threading.Tasks;
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
    public async Task<IActionResult> Get()
    {
      var artists = await artistsService.GetArtistsAsync();
      return Ok(artists);
    }
  }
}