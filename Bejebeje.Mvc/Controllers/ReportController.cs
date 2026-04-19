namespace Bejebeje.Mvc.Controllers;

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Bejebeje.Shared.Domain;
using Bejebeje.Models.Report;
using Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Services.Interfaces;

[Authorize]
public class ReportController : Controller
{
  private readonly ILyricReportsService _reportsService;
  private readonly IBbPointsService _bbPointsService;
  private readonly ILogger<ReportController> _logger;

  public ReportController(
    ILyricReportsService reportsService,
    IBbPointsService bbPointsService,
    ILogger<ReportController> logger)
  {
    _reportsService = reportsService;
    _bbPointsService = bbPointsService;
    _logger = logger;
  }

  [HttpGet]
  [Route("artists/{artistSlug}/lyrics/{lyricSlug}/report")]
  public async Task<IActionResult> Report(string artistSlug, string lyricSlug)
  {
    var viewModel = await _reportsService.GetLyricDetailsForReportAsync(artistSlug, lyricSlug);

    if (viewModel == null)
    {
      _logger.LogDebug("lyric not found for report: {ArtistSlug}/{LyricSlug}", artistSlug, lyricSlug);
      return RedirectToAction("Index", "Home");
    }

    string userId = User.GetUserId().ToString();

    bool hasPending = await _reportsService.HasPendingReportForLyricAsync(userId, viewModel.LyricId);
    viewModel.HasPendingReport = hasPending;

    return View(viewModel);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  [Route("artists/{artistSlug}/lyrics/{lyricSlug}/report")]
  public async Task<IActionResult> SubmitReport(string artistSlug, string lyricSlug, LyricReportFormViewModel formModel)
  {
    if (!ModelState.IsValid)
    {
      // re-fetch lyric details to re-render the form
      var lyricDetails = await _reportsService.GetLyricDetailsForReportAsync(artistSlug, lyricSlug);

      if (lyricDetails == null)
      {
        return RedirectToAction("Index", "Home");
      }

      return View("Report", lyricDetails);
    }

    // re-fetch lyric details for validation and email data
    var viewModel = await _reportsService.GetLyricDetailsForReportAsync(artistSlug, lyricSlug);

    if (viewModel == null)
    {
      _logger.LogDebug("lyric not found during report submission: {ArtistSlug}/{LyricSlug}", artistSlug, lyricSlug);
      return RedirectToAction("Index", "Home");
    }

    string userId = User.GetUserId().ToString();

    // server-side duplicate re-check
    bool hasPending = await _reportsService.HasPendingReportForLyricAsync(userId, viewModel.LyricId);

    if (hasPending)
    {
      _logger.LogDebug("user {UserId} already has pending report for lyric {LyricId}", userId, viewModel.LyricId);
      return RedirectToAction("Report", new { artistSlug, lyricSlug });
    }

    // save the report
    await _reportsService.CreateReportAsync(userId, viewModel.LyricId, formModel.Category.Value, formModel.Comment);

    // award bb points for report submission
    try
    {
      string cognitoUserId = User.GetCognitoUserId();
      string username = User.GetPreferredUsername();

      bool earned = await _bbPointsService.AwardSubmissionPointsAsync(
        cognitoUserId, username, PointActionType.ReportSubmitted,
        viewModel.LyricId, viewModel.LyricTitle, BbPointsConstants.ReportSubmitted);

      TempData["BbPoints:Earned"] = earned;
      TempData["BbPoints:Amount"] = BbPointsConstants.ReportSubmitted;
      TempData["BbPoints:EntityType"] = "report";
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "failed to award bb points for report submission on lyric {LyricId}", viewModel.LyricId);
    }

    // get category display label
    string categoryDisplayLabel = GetCategoryDisplayLabel(formModel.Category.Value);

    // send emails (errors are caught within the service)
    string reporterEmail = User.FindFirstValue("email");

    await _reportsService.SendReportEmailsAsync(
      userId,
      reporterEmail,
      viewModel.LyricTitle,
      viewModel.ArtistName,
      categoryDisplayLabel,
      formModel.Comment);

    // set tempdata for thank-you page
    TempData["LyricTitle"] = viewModel.LyricTitle;
    TempData["ArtistName"] = viewModel.ArtistName;
    TempData["ArtistSlug"] = artistSlug;
    TempData["LyricSlug"] = lyricSlug;

    return RedirectToAction("ThankYou", new { artistSlug, lyricSlug });
  }

  [HttpGet]
  [Route("artists/{artistSlug}/lyrics/{lyricSlug}/report/thank-you")]
  public IActionResult ThankYou(string artistSlug, string lyricSlug)
  {
    string lyricTitle = TempData["LyricTitle"] as string;
    string artistName = TempData["ArtistName"] as string;

    if (string.IsNullOrEmpty(lyricTitle))
    {
      return RedirectToAction("Lyric", "Lyric", new { artistSlug, lyricSlug });
    }

    var viewModel = new LyricReportThankYouViewModel
    {
      LyricTitle = lyricTitle,
      ArtistName = artistName,
      ArtistSlug = TempData["ArtistSlug"] as string ?? artistSlug,
      LyricSlug = TempData["LyricSlug"] as string ?? lyricSlug,
    };

    return View(viewModel);
  }

  private static string GetCategoryDisplayLabel(int category)
  {
    return (ReportCategory)category switch
    {
      ReportCategory.LyricsNotInKurdish => "Lyrics are not in Kurdish",
      ReportCategory.LyricsContainMistakes => "Lyrics contain mistakes",
      ReportCategory.Duplicate => "Duplicate",
      ReportCategory.WrongArtist => "Wrong artist",
      ReportCategory.OffensiveOrInappropriate => "Offensive or inappropriate content",
      _ => "Unknown category",
    };
  }
}
