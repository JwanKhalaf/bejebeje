namespace Bejebeje.Services.Services
{
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Common.Extensions;
  using Bejebeje.DataAccess.Context;
  using Bejebeje.Domain;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.EntityFrameworkCore;

  public class ImagesService : IImagesService
  {
    private readonly IArtistsService artistsService;

    private readonly BbContext context;

    public ImagesService(
      IArtistsService artistsService,
      BbContext context)
    {
      this.artistsService = artistsService;
      this.context = context;
    }

    public async Task<byte[]> GetArtistImageBytesAsync(string artistSlug)
    {
      int artistId = await artistsService.GetArtistIdAsync(artistSlug);

      Artist artist = await context
        .Artists
        .AsNoTracking()
        .Include(x => x.Image)
        .Where(a => a.Slugs.Any(s => s.Name == artistSlug.Standardize()))
        .SingleOrDefaultAsync();

      if (artist.Image == null)
      {
        throw new MissingArtistImageException(artistSlug);
      }

      return artist.Image.Data;
    }
  }
}
