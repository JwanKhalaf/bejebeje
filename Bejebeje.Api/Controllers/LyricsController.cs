namespace Bejebeje.Api.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Common.Extensions;
  using Bejebeje.Services.Services.Interfaces;
  using Bejebeje.ViewModels.Lyric;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;

  [ApiController]
  public class LyricsController : ControllerBase
  {
    private readonly ILyricsService lyricsService;

    private readonly ILogger logger;

    public LyricsController(
      ILyricsService lyricsService,
      ILogger<LyricsController> logger)
    {
      this.lyricsService = lyricsService;
      this.logger = logger;
    }

    [Route("artists/{artistSlug}/[controller]")]
    [HttpGet]
    public async Task<IActionResult> Get(string artistSlug)
    {
      if (string.IsNullOrEmpty(artistSlug))
      {
        throw new ArgumentException("Missing artist slug", nameof(artistSlug));
      }

      try
      {
        IList<LyricCardViewModel> lyrics = await lyricsService.GetLyricsByArtistSlugAsync(artistSlug);

        return Ok(lyrics);
      }
      catch (ArtistNotFoundException exception)
      {
        logger.LogError($"The requested artist was not found. {exception.ToLogData()}");

        return NotFound();
      }
    }
  }
}
