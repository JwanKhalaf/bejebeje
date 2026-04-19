namespace Bejebeje.Mvc.Tests.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Security.Claims;
  using System.Threading.Tasks;
  using Bejebeje.Shared.Domain;
  using Bejebeje.Models.Artist;
  using Bejebeje.Services.Services.Interfaces;
  using FluentAssertions;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Mvc.ViewFeatures;
  using Microsoft.Extensions.Logging;
  using Moq;
  using Mvc.Controllers;
  using Mvc.ViewModels.Artists.CreateBandArtist;
  using Mvc.ViewModels.Artists.CreateIndividualArtist;
  using NUnit.Framework;

  [TestFixture]
  public class ArtistControllerIndividualPointsTests
  {
    private Mock<IArtistsService> _mockArtistsService;
    private Mock<IImagesService> _mockImagesService;
    private Mock<IBbPointsService> _mockPointsService;
    private Mock<ILogger<ArtistController>> _mockLogger;
    private ArtistController _controller;
    private string _testUserId;

    [SetUp]
    public void SetUp()
    {
      _mockArtistsService = new Mock<IArtistsService>();
      _mockImagesService = new Mock<IImagesService>();
      _mockPointsService = new Mock<IBbPointsService>();
      _mockLogger = new Mock<ILogger<ArtistController>>();
      _testUserId = Guid.NewGuid().ToString();

      _controller = new ArtistController(
        _mockArtistsService.Object,
        _mockImagesService.Object,
        _mockPointsService.Object,
        _mockLogger.Object);

      SetupAuthenticatedUser();
    }

    [Test]
    public async Task should_award_1_point_for_individual_artist_without_photo()
    {
      // arrange
      var viewModel = new CreateIndividualArtistViewModel { FirstName = "Sivan" };

      _mockArtistsService
        .Setup(s => s.ArtistExistsAsync(It.IsAny<string>()))
        .ReturnsAsync(false);

      _mockArtistsService
        .Setup(s => s.AddArtistAsync(It.IsAny<CreateIndividualArtistDto>()))
        .ReturnsAsync(new ArtistCreationResult { IsSuccessful = true, ArtistId = 42, PrimarySlug = "sivan" });

      _mockPointsService
        .Setup(s => s.AwardSubmissionPointsAsync(
          _testUserId, "testuser", PointActionType.ArtistSubmitted, 42, "Sivan", 1))
        .ReturnsAsync(true);

      // act
      await _controller.Create(viewModel);

      // assert
      _mockPointsService.Verify(
        s => s.AwardSubmissionPointsAsync(
          _testUserId, "testuser", PointActionType.ArtistSubmitted, 42, "Sivan", 1),
        Times.Once);
    }

    [Test]
    public async Task should_set_tempdata_earned_true_when_points_awarded()
    {
      // arrange
      var viewModel = new CreateIndividualArtistViewModel { FirstName = "Sivan" };

      _mockArtistsService
        .Setup(s => s.ArtistExistsAsync(It.IsAny<string>()))
        .ReturnsAsync(false);

      _mockArtistsService
        .Setup(s => s.AddArtistAsync(It.IsAny<CreateIndividualArtistDto>()))
        .ReturnsAsync(new ArtistCreationResult { IsSuccessful = true, ArtistId = 42, PrimarySlug = "sivan" });

      _mockPointsService
        .Setup(s => s.AwardSubmissionPointsAsync(
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PointActionType>(),
          It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
        .ReturnsAsync(true);

      // act
      await _controller.Create(viewModel);

      // assert
      _controller.TempData["BbPoints:Earned"].Should().Be(true);
      _controller.TempData["BbPoints:Amount"].Should().Be(BbPointsConstants.ArtistSubmittedNoPhoto);
      _controller.TempData["BbPoints:EntityType"].Should().Be("artist");
    }

    [Test]
    public async Task should_set_tempdata_earned_false_when_daily_limit_exceeded()
    {
      // arrange
      var viewModel = new CreateIndividualArtistViewModel { FirstName = "Sivan" };

      _mockArtistsService
        .Setup(s => s.ArtistExistsAsync(It.IsAny<string>()))
        .ReturnsAsync(false);

      _mockArtistsService
        .Setup(s => s.AddArtistAsync(It.IsAny<CreateIndividualArtistDto>()))
        .ReturnsAsync(new ArtistCreationResult { IsSuccessful = true, ArtistId = 42, PrimarySlug = "sivan" });

      _mockPointsService
        .Setup(s => s.AwardSubmissionPointsAsync(
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PointActionType>(),
          It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
        .ReturnsAsync(false);

      // act
      await _controller.Create(viewModel);

      // assert
      _controller.TempData["BbPoints:Earned"].Should().Be(false);
    }

    [Test]
    public async Task should_still_redirect_on_success_when_points_service_throws()
    {
      // arrange
      var viewModel = new CreateIndividualArtistViewModel { FirstName = "Sivan" };

      _mockArtistsService
        .Setup(s => s.ArtistExistsAsync(It.IsAny<string>()))
        .ReturnsAsync(false);

      _mockArtistsService
        .Setup(s => s.AddArtistAsync(It.IsAny<CreateIndividualArtistDto>()))
        .ReturnsAsync(new ArtistCreationResult { IsSuccessful = true, ArtistId = 42, PrimarySlug = "sivan" });

      _mockPointsService
        .Setup(s => s.AwardSubmissionPointsAsync(
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PointActionType>(),
          It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
        .ThrowsAsync(new Exception("db error"));

      // act
      var result = await _controller.Create(viewModel);

      // assert
      var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirect.ActionName.Should().Be("ArtistLyrics");
    }

    private void SetupAuthenticatedUser()
    {
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

      _controller.TempData = new TempDataDictionary(
        _controller.HttpContext,
        Mock.Of<ITempDataProvider>());
    }
  }

  [TestFixture]
  public class ArtistControllerBandPointsTests
  {
    private Mock<IArtistsService> _mockArtistsService;
    private Mock<IImagesService> _mockImagesService;
    private Mock<IBbPointsService> _mockPointsService;
    private Mock<ILogger<ArtistController>> _mockLogger;
    private ArtistController _controller;
    private string _testUserId;

    [SetUp]
    public void SetUp()
    {
      _mockArtistsService = new Mock<IArtistsService>();
      _mockImagesService = new Mock<IImagesService>();
      _mockPointsService = new Mock<IBbPointsService>();
      _mockLogger = new Mock<ILogger<ArtistController>>();
      _testUserId = Guid.NewGuid().ToString();

      _controller = new ArtistController(
        _mockArtistsService.Object,
        _mockImagesService.Object,
        _mockPointsService.Object,
        _mockLogger.Object);

      SetupAuthenticatedUser();
    }

    [Test]
    public async Task should_award_1_point_for_band_artist_without_photo()
    {
      // arrange
      var viewModel = new CreateBandArtistViewModel { BandName = "Koma Amed" };

      _mockArtistsService
        .Setup(s => s.ArtistExistsAsync(It.IsAny<string>()))
        .ReturnsAsync(false);

      _mockArtistsService
        .Setup(s => s.AddBandArtistAsync(It.IsAny<CreateBandArtistDto>()))
        .ReturnsAsync(new ArtistCreationResult { IsSuccessful = true, ArtistId = 99, PrimarySlug = "koma-amed" });

      _mockPointsService
        .Setup(s => s.AwardSubmissionPointsAsync(
          _testUserId, "testuser", PointActionType.ArtistSubmitted, 99, "Koma Amed", 1))
        .ReturnsAsync(true);

      // act
      await _controller.CreateBand(viewModel);

      // assert
      _mockPointsService.Verify(
        s => s.AwardSubmissionPointsAsync(
          _testUserId, "testuser", PointActionType.ArtistSubmitted, 99, "Koma Amed", 1),
        Times.Once);
    }

    [Test]
    public async Task should_set_tempdata_for_band_artist_submission()
    {
      // arrange
      var viewModel = new CreateBandArtistViewModel { BandName = "Koma Amed" };

      _mockArtistsService
        .Setup(s => s.ArtistExistsAsync(It.IsAny<string>()))
        .ReturnsAsync(false);

      _mockArtistsService
        .Setup(s => s.AddBandArtistAsync(It.IsAny<CreateBandArtistDto>()))
        .ReturnsAsync(new ArtistCreationResult { IsSuccessful = true, ArtistId = 99, PrimarySlug = "koma-amed" });

      _mockPointsService
        .Setup(s => s.AwardSubmissionPointsAsync(
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PointActionType>(),
          It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
        .ReturnsAsync(true);

      // act
      await _controller.CreateBand(viewModel);

      // assert
      _controller.TempData["BbPoints:Earned"].Should().Be(true);
      _controller.TempData["BbPoints:EntityType"].Should().Be("artist");
    }

    [Test]
    public async Task should_still_redirect_on_success_when_points_service_throws()
    {
      // arrange
      var viewModel = new CreateBandArtistViewModel { BandName = "Koma Amed" };

      _mockArtistsService
        .Setup(s => s.ArtistExistsAsync(It.IsAny<string>()))
        .ReturnsAsync(false);

      _mockArtistsService
        .Setup(s => s.AddBandArtistAsync(It.IsAny<CreateBandArtistDto>()))
        .ReturnsAsync(new ArtistCreationResult { IsSuccessful = true, ArtistId = 99, PrimarySlug = "koma-amed" });

      _mockPointsService
        .Setup(s => s.AwardSubmissionPointsAsync(
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PointActionType>(),
          It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
        .ThrowsAsync(new Exception("db error"));

      // act
      var result = await _controller.CreateBand(viewModel);

      // assert
      var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirect.ActionName.Should().Be("ArtistLyrics");
    }

    private void SetupAuthenticatedUser()
    {
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

      _controller.TempData = new TempDataDictionary(
        _controller.HttpContext,
        Mock.Of<ITempDataProvider>());
    }
  }
}
