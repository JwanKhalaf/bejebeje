namespace Bejebeje.Services.Services.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.ViewModels.Artist;

  public interface IArtistsService
  {
    Task<IList<ArtistCardViewModel>> GetArtistsAsync();
  }
}
