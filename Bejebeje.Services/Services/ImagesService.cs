namespace Bejebeje.Services.Services
{
  using Interfaces;

  public class ImagesService : IImagesService
  {
    private readonly IArtistsService _artistsService;

    public ImagesService(
      IArtistsService artistsService)
    {
      _artistsService = artistsService;
    }
  }
}
