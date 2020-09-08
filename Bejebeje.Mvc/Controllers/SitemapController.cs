namespace Bejebeje.Mvc.Controllers
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.Models.Sitemap;
  using Microsoft.AspNetCore.Mvc;
  using Services.Services.Interfaces;
  using SimpleMvcSitemap;

  public class SitemapController : Controller
  {
    private readonly ISitemapService sitemapService;

    public SitemapController(ISitemapService sitemapService)
    {
      this.sitemapService = sitemapService;
    }

    public async Task<ActionResult> Index()
    {
      string homeUrl = this.ConvertToSsl(this.Url.Action("Index", "Home"));
      string searchUrl = this.ConvertToSsl(this.Url.Action("Index", "Search"));

      List<SitemapNode> nodes = new List<SitemapNode>
      {

        new SitemapNode(homeUrl),
        new SitemapNode(searchUrl),
      };

      // build urls for artists
      IEnumerable<ArtistUrlViewModel> artistUrls = await this.sitemapService
        .GetAllArtistsAsync();

      foreach (ArtistUrlViewModel url in artistUrls)
      {
        string artistUrl = this.ConvertToSsl(this.Url.Action("ArtistLyrics", "Lyric", new { artistSlug = url.ArtistPrimarySlug }));

        SitemapNode sitemapNode = new SitemapNode(artistUrl);

        nodes.Add(sitemapNode);
      }

      // build urls for lyrics
      IEnumerable<LyricUrlViewModel> lyricUrls = await this.sitemapService
        .GetAllLyricsAsync();

      foreach (LyricUrlViewModel url in lyricUrls)
      {
        string lyricUrl = this.ConvertToSsl(this.Url.Action("Lyric", "Lyric", new { artistSlug = url.ArtistPrimarySlug, lyricSlug = url.LyricPrimarySlug }));

        SitemapNode sitemapNode = new SitemapNode(lyricUrl);

        nodes.Add(sitemapNode);
      }

      ActionResult actionResult = new SitemapProvider()
        .CreateSitemap(new SitemapModel(nodes));

      return actionResult;
    }

    private string ConvertToSsl(string url)
    {
      string sslUrl = url.Replace("http://", "https://");

      return sslUrl;
    }
  }
}
