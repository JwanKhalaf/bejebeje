namespace Bejebeje.Api.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Common.Extensions;
  using Bejebeje.Models.Lyric;
  using Bejebeje.Services.Services.Interfaces;
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
    public async Task<IActionResult> GetLyrics(string artistSlug)
    {
      if (string.IsNullOrEmpty(artistSlug))
      {
        throw new ArgumentNullException(nameof(artistSlug), "Missing artist slug");
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

    [Route("[controller]")]
    [HttpGet]
    public async Task<IActionResult> SearchLyrics(
      [FromQuery] string title,
      int offset = 0,
      int limit = 10)
    {
      PagedLyricSearchResponse response = await lyricsService
        .SearchLyricsAsync(title, offset, limit)
        .ConfigureAwait(false);

      return Ok(response);
    }

    [Route("artists/{artistSlug}/[controller]/{lyricSlug}")]
    [HttpGet]
    public async Task<IActionResult> GetSingleLyric(
      string artistSlug,
      string lyricSlug)
    {
      if (string.IsNullOrEmpty(artistSlug))
      {
        throw new ArgumentNullException(nameof(artistSlug), "Missing artist slug");
      }

      if (string.IsNullOrEmpty(lyricSlug))
      {
        throw new ArgumentNullException(nameof(lyricSlug), "Missing lyric slug");
      }

      try
      {
        LyricResponse lyric = await lyricsService
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
