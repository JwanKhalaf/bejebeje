namespace Bejebeje.Services.Services.Interfaces
{
  using System.Threading.Tasks;
  using Bejebeje.Models.Artist;

  public interface IArtistsService
  {
    Task<int> GetArtistIdAsync(string artistSlug);

    Task<bool> ArtistExistsAsync(string artistSlug);

    Task<ArtistViewModel> GetArtistDetailsAsync(string artistSlug);

    Task<CreateNewArtistResponse> CreateNewArtistAsync(CreateNewArtistRequest request);

    Task<PagedArtistSearchResponse> SearchArtistsAsync(string artistName, int offset, int limit);
  }
}
