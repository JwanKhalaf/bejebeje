namespace Bejebeje.Api.Controllers
{
  using System;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Common.Extensions;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;

  public class ArtistImagesController : ControllerBase
  {
    private readonly IImagesService imagesService;

    private readonly ILogger logger;

    public ArtistImagesController(
      IImagesService imagesService,
      ILogger<ArtistImagesController> logger)
    {
      this.imagesService = imagesService;
      this.logger = logger;
    }

    [Route("artists/{artistSlug}/image")]
    [HttpGet]
    public async Task<IActionResult> GetArtistImage(string artistSlug)
    {
      if (artistSlug == null)
      {
        throw new ArgumentNullException(nameof(artistSlug));
      }

      try
      {
        byte[] imageBytes = await imagesService.GetArtistImageBytesAsync(artistSlug);

        return File(imageBytes, "image/jpeg");
      }
      catch (ArtistNotFoundException exception)
      {
        logger.LogError($"The artist with slug: {exception.ToLogData()} could not be found.");

        return NotFound();
      }
      catch (MissingArtistImageException)
      {
        logger.LogError($"The image for the requested artist could not be found.");

        return NotFound();
      }
    }
  }
}
