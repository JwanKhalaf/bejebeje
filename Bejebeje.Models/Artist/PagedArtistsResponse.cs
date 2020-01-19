namespace Bejebeje.Models.Artist
{
  using System.Collections.Generic;
  using Bejebeje.Models.Paging;

  public class PagedArtistsResponse
  {
    public ICollection<ArtistsResponse> Artists { get; set; } = new List<ArtistsResponse>();

    public PagingResponse Paging { get; set; }
  }
}
