namespace Bejebeje.Mvc.Tests.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Security.Claims;
  using System.Threading.Tasks;
  using Bejebeje.Shared.Domain;
  using Bejebeje.Models.Report;
  using FluentAssertions;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Mvc.ViewFeatures;
  using Microsoft.Extensions.Logging;
  using Moq;
  using Mvc.Controllers;
  using NUnit.Framework;
  using Services.Services.Interfaces;

  [TestFixture]
  public class ReportControllerTests
  {
    private Mock<ILyricReportsService> _mockReportsService;
    private Mock<IBbPointsService> _mockPointsService;
    private Mock<ILogger<ReportController>> _mockLogger;
    private ReportController _controller;
    private string _testUserId;

    [SetUp]
    public void SetUp()
    {
      _mockReportsService = new Mock<ILyricReportsService>();
      _mockPointsService = new Mock<IBbPointsService>();
      _mockLogger = new Mock<ILogger<ReportController>>();
      _controller = new ReportController(
        _mockReportsService.Object,
        _mockPointsService.Object,
        _mockLogger.Object);
      _testUserId = Guid.NewGuid().ToString();

      // set up authenticated user
      var claims = new List<Claim>
      {
        new Claim("sub", _testUserId),
        new Claim("preferred_username", "testuser"),
        new Claim("email", "test@example.com"),
      };

      var identity = new ClaimsIdentity(claims, "TestAuth");
      var principal = new ClaimsPrincipal(identity);

      _controller.ControllerContext = new ControllerContext
      {
        HttpContext = new DefaultHttpContext { User = principal },
      };

      // set up tempdata
      _controller.TempData = new TempDataDictionary(
        _controller.HttpContext,
        Mock.Of<ITempDataProvider>());
    }

    // --- GET Report ---

    [Test]
    public async Task report_get_should_redirect_to_home_when_lyric_not_found()
    {
      // arrange
      _mockReportsService
        .Setup(s => s.GetLyricDetailsForReportAsync("test-artist", "test-lyric"))
        .ReturnsAsync((LyricReportViewModel)null);

      // act
      var result = await _controller.Report("test-artist", "test-lyric");

      // assert
      var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirect.ActionName.Should().Be("Index");
      redirect.ControllerName.Should().Be("Home");
    }

    [Test]
    public async Task report_get_should_always_show_form_regardless_of_daily_count()
    {
      // arrange - no daily limit check means form always shows
      var viewModel = CreateTestLyricReportViewModel();

      _mockReportsService
        .Setup(s => s.GetLyricDetailsForReportAsync("test-artist", "test-lyric"))
        .ReturnsAsync(viewModel);

      _mockReportsService
        .Setup(s => s.HasPendingReportForLyricAsync(_testUserId, 42))
        .ReturnsAsync(false);

      // act
      var result = await _controller.Report("test-artist", "test-lyric");

      // assert
      result.Should().BeOfType<ViewResult>();
    }

    [Test]
    public async Task report_get_should_set_has_pending_report_when_pending_exists()
    {
      // arrange
      var viewModel = CreateTestLyricReportViewModel();

      _mockReportsService
        .Setup(s => s.GetLyricDetailsForReportAsync("test-artist", "test-lyric"))
        .ReturnsAsync(viewModel);

      _mockReportsService
        .Setup(s => s.HasPendingReportForLyricAsync(_testUserId, 42))
        .ReturnsAsync(true);

      // act
      var result = await _controller.Report("test-artist", "test-lyric");

      // assert
      var view = result.Should().BeOfType<ViewResult>().Subject;
      var model = view.Model.Should().BeOfType<LyricReportViewModel>().Subject;
      model.HasPendingReport.Should().BeTrue();
    }

    [Test]
    public async Task report_get_should_return_view_with_form_when_valid()
    {
      // arrange
      var viewModel = CreateTestLyricReportViewModel();

      _mockReportsService
        .Setup(s => s.GetLyricDetailsForReportAsync("test-artist", "test-lyric"))
        .ReturnsAsync(viewModel);

      _mockReportsService
        .Setup(s => s.HasPendingReportForLyricAsync(_testUserId, 42))
        .ReturnsAsync(false);

      // act
      var result = await _controller.Report("test-artist", "test-lyric");

      // assert
      var view = result.Should().BeOfType<ViewResult>().Subject;
      var model = view.Model.Should().BeOfType<LyricReportViewModel>().Subject;
      model.HasPendingReport.Should().BeFalse();
      model.LyricTitle.Should().Be("Test Lyric");
      model.ArtistName.Should().Be("Test Artist");
    }

    // --- POST SubmitReport ---

    [Test]
    public async Task submit_report_should_return_view_when_model_state_invalid()
    {
      // arrange
      var formModel = CreateTestFormModel();
      _controller.ModelState.AddModelError("Category", "Category is required");

      var viewModel = CreateTestLyricReportViewModel();
      _mockReportsService
        .Setup(s => s.GetLyricDetailsForReportAsync("test-artist", "test-lyric"))
        .ReturnsAsync(viewModel);

      // act
      var result = await _controller.SubmitReport("test-artist", "test-lyric", formModel);

      // assert
      result.Should().BeOfType<ViewResult>();
    }

    [Test]
    public async Task submit_report_should_redirect_to_home_when_lyric_not_found()
    {
      // arrange
      var formModel = CreateTestFormModel();

      _mockReportsService
        .Setup(s => s.GetLyricDetailsForReportAsync("test-artist", "test-lyric"))
        .ReturnsAsync((LyricReportViewModel)null);

      // act
      var result = await _controller.SubmitReport("test-artist", "test-lyric", formModel);

      // assert
      var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirect.ActionName.Should().Be("Index");
      redirect.ControllerName.Should().Be("Home");
    }

    [Test]
    public async Task submit_report_should_redirect_to_report_when_duplicate_exists()
    {
      // arrange
      var formModel = CreateTestFormModel();

      var viewModel = CreateTestLyricReportViewModel();
      _mockReportsService
        .Setup(s => s.GetLyricDetailsForReportAsync("test-artist", "test-lyric"))
        .ReturnsAsync(viewModel);

      _mockReportsService
        .Setup(s => s.HasPendingReportForLyricAsync(_testUserId, 42))
        .ReturnsAsync(true);

      // act
      var result = await _controller.SubmitReport("test-artist", "test-lyric", formModel);

      // assert
      var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirect.ActionName.Should().Be("Report");
    }

    [Test]
    public async Task submit_report_should_save_and_redirect_to_thank_you_on_success()
    {
      // arrange
      var formModel = CreateTestFormModel();

      var viewModel = CreateTestLyricReportViewModel();
      _mockReportsService
        .Setup(s => s.GetLyricDetailsForReportAsync("test-artist", "test-lyric"))
        .ReturnsAsync(viewModel);

      _mockReportsService
        .Setup(s => s.HasPendingReportForLyricAsync(_testUserId, 42))
        .ReturnsAsync(false);

      _mockReportsService
        .Setup(s => s.CreateReportAsync(_testUserId, 42, 1, "some comment"))
        .ReturnsAsync(99);

      // act
      var result = await _controller.SubmitReport("test-artist", "test-lyric", formModel);

      // assert
      var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirect.ActionName.Should().Be("ThankYou");
      _mockReportsService.Verify(s => s.CreateReportAsync(_testUserId, 42, 1, "some comment"), Times.Once);
    }

    [Test]
    public async Task submit_report_should_award_bb_points_after_saving()
    {
      // arrange
      var formModel = CreateTestFormModel();

      var viewModel = CreateTestLyricReportViewModel();
      _mockReportsService
        .Setup(s => s.GetLyricDetailsForReportAsync("test-artist", "test-lyric"))
        .ReturnsAsync(viewModel);

      _mockReportsService
        .Setup(s => s.HasPendingReportForLyricAsync(_testUserId, 42))
        .ReturnsAsync(false);

      _mockReportsService
        .Setup(s => s.CreateReportAsync(_testUserId, 42, 1, "some comment"))
        .ReturnsAsync(99);

      _mockPointsService
        .Setup(s => s.AwardSubmissionPointsAsync(
          _testUserId, "testuser", PointActionType.ReportSubmitted, 42, "Test Lyric", 1))
        .ReturnsAsync(true);

      // act
      await _controller.SubmitReport("test-artist", "test-lyric", formModel);

      // assert
      _mockPointsService.Verify(
        s => s.AwardSubmissionPointsAsync(
          _testUserId, "testuser", PointActionType.ReportSubmitted, 42, "Test Lyric", 1),
        Times.Once);

      _controller.TempData["BbPoints:Earned"].Should().Be(true);
      _controller.TempData["BbPoints:Amount"].Should().Be(1);
      _controller.TempData["BbPoints:EntityType"].Should().Be("report");
    }

    [Test]
    public async Task submit_report_should_still_succeed_when_points_service_throws()
    {
      // arrange
      var formModel = CreateTestFormModel();

      var viewModel = CreateTestLyricReportViewModel();
      _mockReportsService
        .Setup(s => s.GetLyricDetailsForReportAsync("test-artist", "test-lyric"))
        .ReturnsAsync(viewModel);

      _mockReportsService
        .Setup(s => s.HasPendingReportForLyricAsync(_testUserId, 42))
        .ReturnsAsync(false);

      _mockReportsService
        .Setup(s => s.CreateReportAsync(_testUserId, 42, 1, "some comment"))
        .ReturnsAsync(99);

      _mockPointsService
        .Setup(s => s.AwardSubmissionPointsAsync(
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PointActionType>(),
          It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
        .ThrowsAsync(new Exception("db error"));

      // act
      var result = await _controller.SubmitReport("test-artist", "test-lyric", formModel);

      // assert
      var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirect.ActionName.Should().Be("ThankYou");
    }

    [Test]
    public async Task submit_report_should_call_send_report_emails()
    {
      // arrange
      var formModel = CreateTestFormModel();

      var viewModel = CreateTestLyricReportViewModel();
      _mockReportsService
        .Setup(s => s.GetLyricDetailsForReportAsync("test-artist", "test-lyric"))
        .ReturnsAsync(viewModel);

      _mockReportsService
        .Setup(s => s.HasPendingReportForLyricAsync(_testUserId, 42))
        .ReturnsAsync(false);

      _mockReportsService
        .Setup(s => s.CreateReportAsync(_testUserId, 42, 1, "some comment"))
        .ReturnsAsync(99);

      // act
      await _controller.SubmitReport("test-artist", "test-lyric", formModel);

      // assert
      _mockReportsService.Verify(
        s => s.SendReportEmailsAsync(
          _testUserId,
          "test@example.com",
          "Test Lyric",
          "Test Artist",
          It.IsAny<string>(),
          "some comment"),
        Times.Once);
    }

    // --- GET ThankYou ---

    [Test]
    public async Task thank_you_should_redirect_to_lyric_when_tempdata_empty()
    {
      // arrange - tempdata is empty by default

      // act
      var result = _controller.ThankYou("test-artist", "test-lyric");

      // assert
      var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirect.ActionName.Should().Be("Lyric");
      redirect.ControllerName.Should().Be("Lyric");
    }

    [Test]
    public void thank_you_should_return_view_when_tempdata_has_data()
    {
      // arrange
      _controller.TempData["LyricTitle"] = "Test Lyric";
      _controller.TempData["ArtistName"] = "Test Artist";
      _controller.TempData["ArtistSlug"] = "test-artist";
      _controller.TempData["LyricSlug"] = "test-lyric";

      // act
      var result = _controller.ThankYou("test-artist", "test-lyric");

      // assert
      var view = result.Should().BeOfType<ViewResult>().Subject;
      var model = view.Model.Should().BeOfType<LyricReportThankYouViewModel>().Subject;
      model.LyricTitle.Should().Be("Test Lyric");
      model.ArtistName.Should().Be("Test Artist");
    }

    // --- edge case: missing email claim ---

    [Test]
    public async Task submit_report_should_pass_null_email_when_claim_missing()
    {
      // arrange - user without email claim
      var claims = new List<Claim>
      {
        new Claim("sub", _testUserId),
        new Claim("preferred_username", "testuser"),
      };

      var identity = new ClaimsIdentity(claims, "TestAuth");
      var principal = new ClaimsPrincipal(identity);

      _controller.ControllerContext = new ControllerContext
      {
        HttpContext = new DefaultHttpContext { User = principal },
      };

      _controller.TempData = new TempDataDictionary(
        _controller.HttpContext,
        Mock.Of<ITempDataProvider>());

      var formModel = CreateTestFormModel();

      var viewModel = CreateTestLyricReportViewModel();
      _mockReportsService
        .Setup(s => s.GetLyricDetailsForReportAsync("test-artist", "test-lyric"))
        .ReturnsAsync(viewModel);

      _mockReportsService
        .Setup(s => s.HasPendingReportForLyricAsync(_testUserId, 42))
        .ReturnsAsync(false);

      _mockReportsService
        .Setup(s => s.CreateReportAsync(_testUserId, 42, 1, "some comment"))
        .ReturnsAsync(99);

      // act
      await _controller.SubmitReport("test-artist", "test-lyric", formModel);

      // assert
      _mockReportsService.Verify(
        s => s.SendReportEmailsAsync(
          _testUserId,
          null,
          "Test Lyric",
          "Test Artist",
          It.IsAny<string>(),
          "some comment"),
        Times.Once);
    }

    // --- edge case: lyric deleted between GET and POST ---

    [Test]
    public async Task submit_report_should_redirect_home_when_lyric_deleted_between_get_and_post()
    {
      // arrange
      var formModel = CreateTestFormModel();

      _mockReportsService
        .Setup(s => s.GetLyricDetailsForReportAsync("test-artist", "test-lyric"))
        .ReturnsAsync((LyricReportViewModel)null);

      // act
      var result = await _controller.SubmitReport("test-artist", "test-lyric", formModel);

      // assert
      var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirect.ActionName.Should().Be("Index");
      redirect.ControllerName.Should().Be("Home");
    }

    // --- helpers ---

    private static LyricReportViewModel CreateTestLyricReportViewModel()
    {
      return new LyricReportViewModel
      {
        LyricId = 42,
        LyricTitle = "Test Lyric",
        LyricBody = "some lyric body text",
        ArtistName = "Test Artist",
        ArtistSlug = "test-artist",
        LyricSlug = "test-lyric",
      };
    }

    private static LyricReportFormViewModel CreateTestFormModel()
    {
      return new LyricReportFormViewModel
      {
        LyricId = 42,
        Category = 1,
        Comment = "some comment",
        ArtistSlug = "test-artist",
        LyricSlug = "test-lyric",
      };
    }
  }
}
