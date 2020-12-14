namespace Bejebeje.Models.Artist
{
  using System.Collections.Generic;
  using Paging;

  public class PagedArtistSearchResponse
  {
    public ICollection<ArtistSearchResponse> Artists { get; set; } = new List<ArtistSearchResponse>();

    public PagingResponse Paging { get; set; }
  }
}
