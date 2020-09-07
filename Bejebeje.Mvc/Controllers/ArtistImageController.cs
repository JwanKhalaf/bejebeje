namespace Bejebeje.Mvc.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Bejebeje.Common.Exceptions;
    using Bejebeje.Common.Extensions;
    using Bejebeje.Services.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

  public class ArtistImageController : Controller
  {
    private readonly IImagesService imagesService;

    private readonly ILogger<ArtistImageController> logger;

    public ArtistImageController(
      IImagesService imagesService,
      ILogger<ArtistImageController> logger)
    {
      this.imagesService = imagesService;
      this.logger = logger;
    }

    [Route("artists/{artistSlug}/image")]
    [HttpGet]
    public async Task<IActionResult> GetArtistImage(
      string artistSlug)
    {
      if (artistSlug == null)
      {
        throw new ArgumentNullException(nameof(artistSlug));
      }

      try
      {
        byte[] imageBytes = await this.imagesService.GetArtistImageBytesAsync(artistSlug);

        return this.File(imageBytes, "image/jpeg");
      }
      catch (ArtistNotFoundException exception)
      {
        this.logger.LogError($"The artist with slug: {exception.ToLogData()} could not be found.");

        return this.NotFound();
      }
      catch (MissingArtistImageException)
      {
        this.logger.LogError($"The image for the requested artist could not be found.");

        return this.NotFound();
      }
    }
  }
}
