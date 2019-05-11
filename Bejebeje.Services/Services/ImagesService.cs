namespace Bejebeje.Services.Services
{
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Common.Extensions;
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

    public async Task<byte[]> GetArtistImageBytesAsync(string artistSlug)
    {
      byte[] imageBytes = await context
        .ArtistImages
        .Where(ai => ai.Artist.Slugs.Any(s => s.Name == artistSlug.Standardize()))
        .Select(ai => ai.Data)
        .SingleOrDefaultAsync();

      if (imageBytes == null)
      {
        throw new ArtistNotFoundException(artistSlug);
      }

      return imageBytes;
    }
  }
}
