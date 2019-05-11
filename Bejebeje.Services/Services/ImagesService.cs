namespace Bejebeje.Services.Services
{
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.DataAccess.Context;
  using Interfaces;
  using Microsoft.EntityFrameworkCore;

  public class ImagesService : IImagesService
  {
    private readonly BbContext context;

    public ImagesService(BbContext context)
    {
      this.context = context;
    }

    public async Task<byte[]> GetArtistImageBytesAsync(int imageId)
    {
      byte[] imageBytes = await context
        .ArtistImages
        .Where(ai => ai.Id == imageId)
        .Select(ai => ai.Data)
        .SingleOrDefaultAsync();

      return imageBytes;
    }
  }
}
