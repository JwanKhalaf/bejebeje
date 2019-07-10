namespace Bejebeje.Services.Services
{
  using System.Collections.Generic;
  using Bejebeje.Common.Extensions;
  using Bejebeje.Domain;
  using Bejebeje.Services.Services.Interfaces;
  using NodaTime;

  public class ArtistSlugsService : IArtistSlugsService
  {
    public IEnumerable<ArtistSlug> BuildArtistSlugs(string artistFullName)
    {
      List<ArtistSlug> artistSlugs = new List<ArtistSlug>();

      string artistFullNameLowercase = artistFullName.Trim().ToLower();

      ArtistSlug artistSlug = new ArtistSlug
      {
        Name = artistFullNameLowercase.NormalizeStringForUrl(),
        CreatedAt = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc(),
        IsPrimary = true,
      };

      artistSlugs.Add(artistSlug);

      return artistSlugs;
    }
  }
}
