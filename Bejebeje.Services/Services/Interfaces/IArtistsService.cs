namespace Bejebeje.Services.Services.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.ViewModels.Artist;

  public interface IArtistsService
  {
    Task<int> GetArtistIdAsync(string artistSlug);

    Task<ArtistDetailsViewModel> GetArtistDetailsAsync(string artistSlug);

    Task<IList<ArtistCardViewModel>> GetArtistsAsync();
  }
}
