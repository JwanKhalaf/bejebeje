namespace Bejebeje.Services.Services.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.Models.Artist;
  using Models.Search;

  public interface IArtistsService
  {
    Task<int> GetArtistIdAsync(
      string artistSlug);

    Task<bool> ArtistExistsAsync(
      string artistSlug);

    Task<ArtistViewModel> GetArtistDetailsAsync(
      string artistSlug);

    Task<CreateNewArtistResponse> CreateNewArtistAsync(
      CreateNewArtistRequest request);

    Task<IEnumerable<SearchArtistResultViewModel>> SearchArtistsAsync(
      string artistName);
  }
}
