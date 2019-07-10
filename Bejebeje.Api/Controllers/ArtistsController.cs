namespace Bejebeje.Api.Controllers
{
  using System;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Common.Extensions;
  using Bejebeje.Models.Artist;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;

  [ApiController]
  public class ArtistsController : ControllerBase
  {
    private readonly IArtistsService artistsService;

    private readonly ILogger logger;

    public ArtistsController(
      IArtistsService artistsService,
      ILogger<ArtistsController> logger)
    {
      this.artistsService = artistsService;
      this.logger = logger;
    }

    [Route("[controller]")]
    [HttpGet]
    public async Task<IActionResult> GetArtists([FromQuery] string name, int offset = 0, int limit = 10)
    {
      PagedArtistsResponse artistsResponse;

      if (string.IsNullOrEmpty(name))
      {
        artistsResponse = await artistsService
        .GetArtistsAsync(offset, limit)
        .ConfigureAwait(false);
      }
      else
      {
        artistsResponse = await artistsService
        .SearchArtistsAsync(name, offset, limit)
        .ConfigureAwait(false);
      }

      return Ok(artistsResponse);
    }

    [Route("[controller]/{artistSlug}")]
    [HttpGet]
    public async Task<IActionResult> GetArtistDetails(string artistSlug)
    {
      if (string.IsNullOrEmpty(artistSlug))
      {
        throw new ArgumentNullException(nameof(artistSlug));
      }

      try
      {
        ArtistDetailsResponse artistDetails = await artistsService
          .GetArtistDetailsAsync(artistSlug)
          .ConfigureAwait(false);

        return Ok(artistDetails);
      }
      catch (ArtistNotFoundException exception)
      {
        logger.LogError($"The requested artist was not found. {exception.ToLogData()}");

        return NotFound();
      }
    }

    [Route("[controller]")]
    [HttpPost]
    public async Task<IActionResult> AddNewArtist(AddNewArtistRequest request)
    {
      try
      {
        AddNewArtistResponse response = await artistsService.CreateNewArtistAsync(request);

        return Created(response.Uri, response);
      }
      catch (ArtistExistsException exception)
      {
        logger.LogError(exception, $"The artist already exists.");

        return BadRequest();
      }
    }
  }
}