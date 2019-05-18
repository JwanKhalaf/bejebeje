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

    [Route("v{version:apiVersion}/artists/{artistSlug}/[controller]")]
    [HttpGet]
    public async Task<IActionResult> GetLyrics(string artistSlug)
    {
      if (string.IsNullOrEmpty(artistSlug))
      {
        throw new ArgumentNullException("Missing artist slug", nameof(artistSlug));
      }

      try
      {
        IList<LyricCardViewModel> lyrics = await lyricsService
          .GetLyricsAsync(artistSlug)
          .ConfigureAwait(false);

        return Ok(lyrics);
      }
      catch (ArtistNotFoundException exception)
      {
        logger.LogError($"The requested artist was not found. {exception.ToLogData()}");

        return NotFound();
      }
    }

    [Route("v{version:apiVersion}/artists/{artistSlug}/[controller]/{lyricSlug}")]
    [HttpGet]
    public async Task<IActionResult> GetSingleLyric(string artistSlug, string lyricSlug)
    {
      if (string.IsNullOrEmpty(artistSlug))
      {
        throw new ArgumentNullException("Missing artist slug", nameof(artistSlug));
      }

      if (string.IsNullOrEmpty(lyricSlug))
      {
        throw new ArgumentNullException("Missing lyric slug", nameof(lyricSlug));
      }

      try
      {
        LyricViewModel lyric = await lyricsService
          .GetSingleLyricAsync(artistSlug, lyricSlug)
          .ConfigureAwait(false);

        return Ok(lyric);
      }
      catch (ArtistNotFoundException exception)
      {
        logger.LogError($"The requested artist was not found. {exception.ToLogData()}");

        return NotFound();
      }
      catch (LyricNotFoundException exception)
      {
        logger.LogError($"The requested lyric was not found. {exception.ToLogData()}");

        return NotFound();
      }
    }
  }
}
