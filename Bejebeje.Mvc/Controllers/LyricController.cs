namespace Bejebeje.Mvc.Controllers
{
  using System.Threading.Tasks;
  using Bejebeje.Models.Artist;
  using Bejebeje.Models.Lyric;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.AspNetCore.Mvc;

  public class LyricController : Controller
  {
    private readonly ILyricsService _lyricsService;

    public LyricController(
      ILyricsService lyricsService)
    {
      _lyricsService = lyricsService;
    }

    [Route("artists/{artistSlug}/lyrics")]
    public async Task<IActionResult> ArtistLyrics(
      string artistSlug)
    {
      ArtistLyricsViewModel viewModel = await _lyricsService
        .GetLyricsAsync(artistSlug);

      return View(viewModel);
    }

    [Route("artists/{artistSlug}/lyrics/{lyricSlug}")]
    public async Task<IActionResult> Lyric(
      string artistSlug,
      string lyricSlug)
    {
      LyricDetailsViewModel viewModel = await _lyricsService
        .GetSingleLyricAsync(artistSlug, lyricSlug);

      return View(viewModel);
    }
  }
}
