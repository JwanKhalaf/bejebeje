namespace Bejebeje.Api.Controllers
{
  using System.Threading.Tasks;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.AspNetCore.Mvc;

  [Route("v{version:apiVersion}/[controller]")]
  [ApiController]
  public class ArtistsController : ControllerBase
  {
    private readonly IArtistsService artistsService;

    public ArtistsController(IArtistsService artistService)
    {
      artistsService = artistService;
    }

    [HttpGet]
    public async Task<IActionResult> GetArtists()
    {
      var artists = await artistsService
        .GetArtistsAsync()
        .ConfigureAwait(false);

      return Ok(artists);
    }
  }
}