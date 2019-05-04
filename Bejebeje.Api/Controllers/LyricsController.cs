namespace Bejebeje.Api.Controllers
{
  using System;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.AspNetCore.Mvc;

  [ApiController]
  public class LyricsController : ControllerBase
  {
    private readonly ILyricsService lyricsService;

    public LyricsController(ILyricsService lyricsService)
    {
      this.lyricsService = lyricsService;
    }

    [Route("artists/{artistSlug}/[controller]")]
    [HttpGet]
    public IActionResult Get(string artistSlug)
    {
      if (string.IsNullOrEmpty(artistSlug))
      {
        throw new ArgumentException("Missing artist slug", nameof(artistSlug));
      }

      var lyrics = lyricsService.GetLyricsByArtistSlug(artistSlug);
      return Ok(lyrics);
    }
  }
}
