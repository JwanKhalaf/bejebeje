namespace Bejebeje.Services.Services.Interfaces
{
  using System.Collections.Generic;
  using Bejebeje.ViewModels.Artist;

  public interface IArtistsService
  {
    IList<ArtistCardViewModel> GetArtists();
  }
}
