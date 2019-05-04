namespace Bejebeje.Services.Services
{
  using System.Collections.Generic;
  using System.Linq;
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

    public IList<ArtistCardViewModel> GetArtists()
    {
      List<ArtistCardViewModel> artistCards = context
      .Artists
      .AsNoTracking()
      .Select(x => new ArtistCardViewModel
      {
        FirstName = x.FirstName,
        LastName = x.LastName,
        Slug = x.Slugs.Where(y => y.IsPrimary).First().Name,
        ImageUrl = x.ImageUrl
      })
      .ToList();

      return artistCards;
    }
  }
}
