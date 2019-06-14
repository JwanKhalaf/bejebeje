namespace Bejebeje.Models.Artist
{
  using System.Collections.Generic;
  using Bejebeje.Models.ArtistSlug;

  public class ArtistsResponse
  {
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public int ImageId { get; set; }

    public ICollection<ArtistSlugResponse> Slugs { get; set; }
  }
}
