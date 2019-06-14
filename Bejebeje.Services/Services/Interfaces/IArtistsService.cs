namespace Bejebeje.Services.Services.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.Models.Artist;

  public interface IArtistsService
  {
    Task<int> GetArtistIdAsync(string artistSlug);

    Task<ArtistDetailsResponse> GetArtistDetailsAsync(string artistSlug);

    Task<PagedArtistsResponse> GetArtistsAsync(int offset, int limit);

    Task<ICollection<ArtistsResponse>> SearchArtistsAsync(string artistName);
  }
}
