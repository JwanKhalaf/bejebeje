using Bejebeje.ViewModels.Artist;
using System.Collections.Generic;

namespace Bejebeje.Services.Services.Interfaces
{
  public interface IArtistsService
  {
    IList<ArtistCardViewModel> GetArtistCards();
  }
}
