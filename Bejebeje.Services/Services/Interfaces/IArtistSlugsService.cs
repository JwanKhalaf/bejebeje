namespace Bejebeje.Services.Services.Interfaces
{
  using System.Threading.Tasks;
  using Bejebeje.Shared.Domain;
  using Models.ArtistSlug;

  public interface IArtistSlugsService
  {
    string GetArtistSlug(
      string artistFullName);

    ArtistSlug BuildArtistSlug(
      string artistFullName);

    Task AddArtistSlugAsync(
      ArtistSlugCreateViewModel artistSlug);
  }
}
