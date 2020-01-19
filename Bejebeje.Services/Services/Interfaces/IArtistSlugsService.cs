namespace Bejebeje.Services.Services.Interfaces
{
  using Bejebeje.Domain;

  public interface IArtistSlugsService
  {
    string GetArtistSlug(string artistFullName);

    ArtistSlug BuildArtistSlug(string artistFullName);
  }
}
