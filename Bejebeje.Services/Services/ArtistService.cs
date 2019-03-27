using Bejebeje.Services.Services.Interfaces;
using Bejebeje.ViewModels.Artist;
using System.Collections.Generic;

namespace Bejebeje.Services.Services
{
  public class ArtistService : IArtistService
  {
    public IList<ArtistCardViewModel> GetArtistCards()
    {
      List<ArtistCardViewModel> artistCards = new List<ArtistCardViewModel>();

      ArtistCardViewModel artistOne = new ArtistCardViewModel();
      artistOne.FirstName = "Şivan";
      artistOne.LastName = "Perwer";
      artistOne.ImageUrl = "https://placehold.it/100x100";
      artistOne.Slug = "sivan-perwer";

      artistCards.Add(artistOne);
      return artistCards;
    }
  }
}
