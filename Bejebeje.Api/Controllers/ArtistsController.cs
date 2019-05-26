namespace Bejebeje.Api.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Common.Extensions;
  using Bejebeje.Services.Services.Interfaces;
  using Bejebeje.ViewModels.Artist;
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

    [Route("v{version:apiVersion}/[controller]")]
    [HttpGet]
    public async Task<IActionResult> GetArtists([FromQuery] string name)
    {
      IList<ArtistCardViewModel> artists;

      artists = await artistsService
        .GetArtistsAsync(name)
        .ConfigureAwait(false);

      return Ok(artists);
    }

    [Route("v{version:apiVersion}/[controller]/{artistSlug}")]
    public async Task<IActionResult> GetArtistDetails(string artistSlug)
    {
      if (string.IsNullOrEmpty(artistSlug))
      {
        throw new ArgumentNullException(nameof(artistSlug));
      }

      try
      {
        ArtistDetailsViewModel artistDetails = await artistsService
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
  }
}