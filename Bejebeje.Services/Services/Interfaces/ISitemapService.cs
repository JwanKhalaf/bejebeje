namespace Bejebeje.Services.Services.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Models.Sitemap;

  public interface ISitemapService
  {
    Task<IEnumerable<LyricUrlViewModel>> GetAllLyricsAsync();

    Task<IEnumerable<ArtistUrlViewModel>> GetAllArtistsAsync();
  }
}
