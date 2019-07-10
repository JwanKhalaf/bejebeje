namespace Bejebeje.Services.Services.Interfaces
{
  using System.Threading.Tasks;
  using Bejebeje.Models.Artist;

  public interface IArtistsService
  {
    Task<int> GetArtistIdAsync(string artistSlug);

    Task<ArtistDetailsResponse> GetArtistDetailsAsync(string artistSlug);

    Task<PagedArtistsResponse> GetArtistsAsync(int offset, int limit);

    Task<AddNewArtistResponse> CreateNewArtistAsync(AddNewArtistRequest request);

    Task<PagedArtistsResponse> SearchArtistsAsync(string artistName, int offset, int limit);
  }
}
