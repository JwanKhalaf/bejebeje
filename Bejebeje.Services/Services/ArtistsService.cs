namespace Bejebeje.Services.Services
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.DataAccess.Context;
  using Bejebeje.Services.Services.Interfaces;
  using Bejebeje.ViewModels.Artist;
  using Microsoft.EntityFrameworkCore;

  public class ArtistsService : IArtistsService
  {
    private readonly BbContext context;

    public ArtistsService(BbContext context)
    {
      this.context = context;
    }

    public async Task<IList<ArtistCardViewModel>> GetArtistsAsync()
    {
      List<ArtistCardViewModel> artistCards = await context
      .Artists
      .AsNoTracking()
      .Select(x => new ArtistCardViewModel
      {
        FirstName = x.FirstName,
        LastName = x.LastName,
        Slug = x.Slugs.Where(y => y.IsPrimary).First().Name,
        ImageId = x.Image == null ? 0 : x.Image.Id
      })
      .ToListAsync();

      return artistCards;
    }
  }
}
