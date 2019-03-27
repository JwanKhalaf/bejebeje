using Bejebeje.Services.Services.Interfaces;
using Bejebeje.ViewModels.Artist;
using System.Collections.Generic;

namespace Bejebeje.Services.Services
{
  public class ArtistsService : IArtistsService
  {
    public IList<ArtistCardViewModel> GetArtistCards()
    {
      List<ArtistCardViewModel> artistCards = new List<ArtistCardViewModel>();

      ArtistCardViewModel artistOne = new ArtistCardViewModel();
      artistOne.FirstName = "Şivan";
      artistOne.LastName = "Perwer";
      artistOne.ImageUrl = "https://placehold.it/100x100";
      artistOne.Slug = "sivan-perwer";

      ArtistCardViewModel artistTwo = new ArtistCardViewModel();
      artistTwo.FirstName = "Ciwan";
      artistTwo.LastName = "Haco";
      artistTwo.ImageUrl = "https://placehold.it/100x100";
      artistTwo.Slug = "ciwan-haco";

      artistCards.Add(artistOne);
      artistCards.Add(artistTwo);
      return artistCards;
    }
  }
}
