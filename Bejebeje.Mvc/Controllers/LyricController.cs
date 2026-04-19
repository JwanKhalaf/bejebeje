namespace Bejebeje.Mvc.Controllers;

using System;
using System.Threading.Tasks;
using Bejebeje.Shared.Domain;
using Bejebeje.Models.Artist;
using Bejebeje.Models.Lyric;
using Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Services.Interfaces;

public class LyricController : Controller
{
  private readonly IArtistsService _artistsService;
  private readonly ILyricsService _lyricsService;
  private readonly IBbPointsService _bbPointsService;
  private readonly ILogger<LyricController> _logger;

  public LyricController(
    IArtistsService artistsService,
    ILyricsService lyricsService,
    IBbPointsService bbPointsService,
    ILogger<LyricController> logger)
  {
    _artistsService = artistsService;
    _lyricsService = lyricsService;
    _bbPointsService = bbPointsService;
    _logger = logger;
  }

  [Route("artists/{artistSlug}/lyrics")]
  public async Task<IActionResult> ArtistLyrics(string artistSlug)
  {
    string userId = User.Identity.IsAuthenticated
      ? User.GetUserId().ToString()
      : string.Empty;

    ArtistLyricsViewModel viewModel = await _lyricsService
      .GetLyricsAsync(artistSlug, userId);

    if (viewModel == null)
    {
      return RedirectToAction("Index", "Home");
    }

    return View(viewModel);
  }

  [Route("artists/{artistSlug}/lyrics/{lyricSlug}")]
  public async Task<IActionResult> Lyric(string artistSlug, string lyricSlug)
  {
    string userId = User.Identity.IsAuthenticated
      ? User.GetUserId().ToString()
      : string.Empty;

    LyricDetailsViewModel viewModel = await _lyricsService
      .GetSingleLyricAsync(artistSlug, lyricSlug, userId);

    viewModel.PrimarySlug = lyricSlug;

    // populate submitter bb points data
    try
    {
      if (!string.IsNullOrEmpty(viewModel.SubmitterUserId))
      {
        var submitterPoints = await _bbPointsService.GetSubmitterPointsAsync(viewModel.SubmitterUserId);
        viewModel.SubmitterPoints = submitterPoints.TotalPoints;
        viewModel.SubmitterLabel = submitterPoints.ContributorLabel;
        viewModel.SubmitterProfileUrl = !string.IsNullOrEmpty(submitterPoints.Slug)
          ? $"/profile/{submitterPoints.Slug}"
          : null;

        // use the resolved username from the points service if it's more reliable
        if (!string.IsNullOrEmpty(submitterPoints.Username))
        {
          viewModel.SubmitterUsername = submitterPoints.Username;
        }
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "failed to fetch submitter points for lyric {ArtistSlug}/{LyricSlug}", artistSlug, lyricSlug);
    }

    return View(viewModel);
  }

  [Authorize]
  [Route("lyrics/like/{lyricId}")]
  public async Task<IActionResult> Like(string artistSlug, string lyricSlug, int lyricId)
  {
    try
    {
      string userId = User.GetUserId().ToString();

      await _lyricsService.LikeLyricAsync(userId, lyricId);

      return RedirectToAction("Lyric", new { artistSlug, lyricSlug });
    }
    catch
    {
      return RedirectToAction("Lyric", new { artistSlug, lyricSlug });
    }
  }

  [Authorize]
  [HttpGet]
  [Route("artists/{artistSlug}/lyrics/new")]
  public async Task<IActionResult> Create(string artistSlug)
  {
    string userId = User
      .GetUserId()
      .ToString();

    ArtistViewModel artist = await _artistsService
      .GetArtistDetailsAsync(artistSlug, userId);

    CreateLyricViewModel viewModel = new CreateLyricViewModel();
    viewModel.Artist = artist;

    return View(viewModel);
  }

  [Authorize]
  [HttpPost]
  [Route("artists/{artistSlug}/lyrics/new")]
  public async Task<IActionResult> Create(CreateLyricViewModel viewModel)
  {
    string userId = User
      .GetUserId()
      .ToString();

    viewModel.UserId = userId;

    LyricCreateResultViewModel result = await _lyricsService
      .AddLyricAsync(viewModel);

    // award bb points for lyric submission
    try
    {
      string cognitoUserId = User.GetCognitoUserId();
      string username = User.GetPreferredUsername();

      bool earned = await _bbPointsService.AwardSubmissionPointsAsync(
        cognitoUserId, username, PointActionType.LyricSubmitted,
        result.LyricId, viewModel.Title, BbPointsConstants.LyricSubmitted);

      TempData["BbPoints:Earned"] = earned;
      TempData["BbPoints:Amount"] = BbPointsConstants.LyricSubmitted;
      TempData["BbPoints:EntityType"] = "lyric";
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "failed to award bb points for lyric creation {LyricId}", result.LyricId);
    }

    return RedirectToAction("Like", "Lyric",
      new { artistSlug = result.ArtistSlug, lyricSlug = result.LyricSlug, lyricId = result.LyricId });
  }
}