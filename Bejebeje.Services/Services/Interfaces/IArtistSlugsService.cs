namespace Bejebeje.Services.Services.Interfaces
{
  using Domain;

  public interface IArtistSlugsService
  {
    string GetArtistSlug(string artistFullName);

    ArtistSlug BuildArtistSlug(string artistFullName);
  }
}
