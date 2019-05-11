namespace Bejebeje.Api.Controllers
{
  using System.Threading.Tasks;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.AspNetCore.Mvc;

  public class ArtistImagesController : ControllerBase
  {
    private IImagesService imagesService;

    public ArtistImagesController(IImagesService imagesService)
    {
      this.imagesService = imagesService;
    }

    [Route("images/{imageId}")]
    public async Task<IActionResult> Get(int imageId)
    {
      byte[] imageBytes = await imagesService.GetArtistImageBytesAsync(imageId);
      return File(imageBytes, "image/jpeg");
    }
  }
}
