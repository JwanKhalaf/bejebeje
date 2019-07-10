namespace Bejebeje.Services.Services.Interfaces
{
  using System.Collections.Generic;
  using Bejebeje.Domain;

  public interface IArtistSlugsService
  {
    IEnumerable<ArtistSlug> BuildArtistSlugs(string artistFullName);
  }
}
