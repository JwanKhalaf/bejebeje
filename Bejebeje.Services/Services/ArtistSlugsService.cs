namespace Bejebeje.Services.Services
{
  using System;
  using Bejebeje.Common.Extensions;
  using Bejebeje.Domain;
  using Bejebeje.Services.Services.Interfaces;
  using NodaTime;

  public class ArtistSlugsService : IArtistSlugsService
  {
    public string GetArtistSlug(
      string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        throw new ArgumentNullException(nameof(name));
      }

      string artistFullNameLowercase = name.Trim().ToLower();

      return artistFullNameLowercase.NormalizeStringForUrl();
    }

    public ArtistSlug BuildArtistSlug(
      string artistFullName)
    {
      if (string.IsNullOrEmpty(artistFullName))
      {
        throw new ArgumentNullException(nameof(artistFullName));
      }

      ArtistSlug artistSlug = new ArtistSlug
      {
        Name = GetArtistSlug(artistFullName),
        CreatedAt = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc(),
        IsPrimary = true,
      };

      return artistSlug;
    }
  }
}
