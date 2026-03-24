namespace Bejebeje.Mvc.Tests.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Security.Claims;
  using System.Threading.Tasks;
  using Bejebeje.Models.BbPoints;
  using Bejebeje.Services.Services.Interfaces;
  using FluentAssertions;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;
  using Moq;
  using Mvc.Controllers;
  using NUnit.Framework;

  [TestFixture]
  public class ProfileControllerOwnProfileTests
  {
    private Mock<IBbPointsService> _mockPointsService;
    private Mock<ILogger<ProfileController>> _mockLogger;
    private ProfileController _controller;
    private string _testUserId;

    [SetUp]
    public void SetUp()
    {
      _mockPointsService = new Mock<IBbPointsService>();
      _mockLogger = new Mock<ILogger<ProfileController>>();
      _testUserId = Guid.NewGuid().ToString();

      _controller = new ProfileController(_mockPointsService.Object, _mockLogger.Object);
      SetupAuthenticatedUser();
    }

    [Test]
    public async Task should_return_view_with_own_profile_data()
    {
      // arrange
      var profileData = new OwnProfileViewModel
      {
        TotalPoints = 150,
        ContributorLabel = "Contributor",
        ArtistSubmissionPoints = 10,
        LyricSubmissionPoints = 50,
        Username = "testuser",
      };

      _mockPointsService
        .Setup(s => s.GetOwnProfileDataAsync(_testUserId))
        .ReturnsAsync(profileData);

      // act
      var result = await _controller.Index();

      // assert
      var view = result.Should().BeOfType<ViewResult>().Subject;
      var model = view.Model.Should().BeOfType<OwnProfileViewModel>().Subject;
      model.TotalPoints.Should().Be(150);
      model.ContributorLabel.Should().Be("Contributor");
    }

    [Test]
    public async Task should_call_service_with_correct_user_id()
    {
      // arrange
      _mockPointsService
        .Setup(s => s.GetOwnProfileDataAsync(_testUserId))
        .ReturnsAsync(new OwnProfileViewModel());

      // act
      await _controller.Index();

      // assert
      _mockPointsService.Verify(s => s.GetOwnProfileDataAsync(_testUserId), Times.Once);
    }

    private void SetupAuthenticatedUser()
    {
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
    }
  }

  [TestFixture]
  public class ProfileControllerPublicProfileTests
  {
    private Mock<IBbPointsService> _mockPointsService;
    private Mock<ILogger<ProfileController>> _mockLogger;
    private ProfileController _controller;
    private string _testUserId;

    [SetUp]
    public void SetUp()
    {
      _mockPointsService = new Mock<IBbPointsService>();
      _mockLogger = new Mock<ILogger<ProfileController>>();
      _testUserId = Guid.NewGuid().ToString();

      _controller = new ProfileController(_mockPointsService.Object, _mockLogger.Object);
      SetupAuthenticatedUser("ownuser");
    }

    [Test]
    public async Task should_redirect_to_own_profile_when_username_matches_authenticated_user()
    {
      // arrange
      _mockPointsService
        .Setup(s => s.GetOwnProfileDataAsync(_testUserId))
        .ReturnsAsync(new OwnProfileViewModel { Username = "ownuser" });

      // act
      var result = await _controller.Public("ownuser");

      // assert
      var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirect.ActionName.Should().Be("Index");
    }

    [Test]
    public async Task should_return_404_when_user_not_found()
    {
      // arrange
      _mockPointsService
        .Setup(s => s.GetPublicProfileDataAsync("nonexistent"))
        .ReturnsAsync((PublicProfileViewModel)null);

      // act
      var result = await _controller.Public("nonexistent");

      // assert
      result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task should_return_view_with_public_profile_data()
    {
      // arrange
      var publicProfile = new PublicProfileViewModel
      {
        Username = "otheruser",
        TotalPoints = 300,
        ContributorLabel = "Regular Contributor",
        ArtistsSubmittedCount = 5,
        LyricsSubmittedCount = 20,
      };

      _mockPointsService
        .Setup(s => s.GetPublicProfileDataAsync("otheruser"))
        .ReturnsAsync(publicProfile);

      // act
      var result = await _controller.Public("otheruser");

      // assert
      var view = result.Should().BeOfType<ViewResult>().Subject;
      var model = view.Model.Should().BeOfType<PublicProfileViewModel>().Subject;
      model.TotalPoints.Should().Be(300);
      model.ContributorLabel.Should().Be("Regular Contributor");
      model.ArtistsSubmittedCount.Should().Be(5);
      model.LyricsSubmittedCount.Should().Be(20);
    }

    [Test]
    public async Task should_return_public_view_for_anonymous_user()
    {
      // arrange - anonymous user
      var identity = new ClaimsIdentity(); // not authenticated
      var principal = new ClaimsPrincipal(identity);
      _controller.ControllerContext = new ControllerContext
      {
        HttpContext = new DefaultHttpContext { User = principal },
      };

      var publicProfile = new PublicProfileViewModel
      {
        Username = "someuser",
        TotalPoints = 50,
        ContributorLabel = "Contributor",
      };

      _mockPointsService
        .Setup(s => s.GetPublicProfileDataAsync("someuser"))
        .ReturnsAsync(publicProfile);

      // act
      var result = await _controller.Public("someuser");

      // assert
      var view = result.Should().BeOfType<ViewResult>().Subject;
      view.Model.Should().BeOfType<PublicProfileViewModel>();
    }

    private void SetupAuthenticatedUser(string preferredUsername)
    {
      var claims = new List<Claim>
      {
        new Claim("sub", _testUserId),
        new Claim("preferred_username", preferredUsername),
      };

      var identity = new ClaimsIdentity(claims, "TestAuth");
      var principal = new ClaimsPrincipal(identity);

      _controller.ControllerContext = new ControllerContext
      {
        HttpContext = new DefaultHttpContext { User = principal },
      };
    }
  }
}
