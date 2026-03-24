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
      SetupAuthenticatedUser();
    }

    [Test]
    public async Task should_redirect_to_own_profile_when_slug_belongs_to_authenticated_user()
    {
      // arrange — the slug resolves to a profile with the same cognito user id
      _mockPointsService
        .Setup(s => s.GetPublicProfileDataAsync("ali-fm"))
        .ReturnsAsync(new PublicProfileViewModel
        {
          Username = "ali fm",
          CognitoUserId = _testUserId,
          TotalPoints = 100,
        });

      // act
      var result = await _controller.Public("ali-fm");

      // assert
      var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirect.ActionName.Should().Be("Index");
    }

    [Test]
    public async Task should_return_view_when_slug_belongs_to_another_user()
    {
      // arrange
      _mockPointsService
        .Setup(s => s.GetPublicProfileDataAsync("other-user"))
        .ReturnsAsync(new PublicProfileViewModel
        {
          Username = "other user",
          CognitoUserId = "different-cognito-id",
          TotalPoints = 300,
          ContributorLabel = "Regular Contributor",
          ArtistsSubmittedCount = 5,
          LyricsSubmittedCount = 20,
        });

      // act
      var result = await _controller.Public("other-user");

      // assert
      var view = result.Should().BeOfType<ViewResult>().Subject;
      var model = view.Model.Should().BeOfType<PublicProfileViewModel>().Subject;
      model.TotalPoints.Should().Be(300);
      model.ContributorLabel.Should().Be("Regular Contributor");
      model.ArtistsSubmittedCount.Should().Be(5);
      model.LyricsSubmittedCount.Should().Be(20);
    }

    [Test]
    public async Task should_return_404_when_slug_not_found()
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
        CognitoUserId = "some-cognito-id",
        TotalPoints = 50,
        ContributorLabel = "Contributor",
      };

      _mockPointsService
        .Setup(s => s.GetPublicProfileDataAsync("some-user"))
        .ReturnsAsync(publicProfile);

      // act
      var result = await _controller.Public("some-user");

      // assert
      var view = result.Should().BeOfType<ViewResult>().Subject;
      view.Model.Should().BeOfType<PublicProfileViewModel>();
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
}
