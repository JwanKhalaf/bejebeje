namespace Bejebeje.Mvc.Tests.Controllers
{
  using System.Collections.Generic;
  using System.Security.Claims;
  using System.Threading.Tasks;
  using Bejebeje.Models.Artist;
  using Bejebeje.Models.Homepage;
  using FluentAssertions;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Mvc;
  using Moq;
  using Mvc.Controllers;
  using NUnit.Framework;
  using Services.Services.Interfaces;

  [TestFixture]
  public class HomeControllerTests
  {
    private Mock<IHomepageService> _mockHomepageService;
    private HomeController _controller;

    [SetUp]
    public void Setup()
    {
      _mockHomepageService = new Mock<IHomepageService>();
    }

    [Test]
    public async Task should_return_view_result_with_homepage_view_model()
    {
      // arrange
      var viewModel = new HomepageViewModel
      {
        IsAuthenticated = false,
        OpportunityCards = new List<OpportunityCardViewModel>().AsReadOnly(),
        FemaleArtists = new List<RandomFemaleArtistItemViewModel>(),
      };

      _mockHomepageService
        .Setup(x => x.GetHomepageViewModelAsync(false))
        .ReturnsAsync(viewModel);

      _controller = CreateController(isAuthenticated: false);

      // act
      var result = await _controller.Index();

      // assert
      var view = result.Should().BeOfType<ViewResult>().Subject;
      view.Model.Should().BeOfType<HomepageViewModel>();
    }

    [Test]
    public async Task should_pass_is_authenticated_true_when_user_is_authenticated()
    {
      // arrange
      var viewModel = new HomepageViewModel { IsAuthenticated = true };

      _mockHomepageService
        .Setup(x => x.GetHomepageViewModelAsync(true))
        .ReturnsAsync(viewModel);

      _controller = CreateController(isAuthenticated: true);

      // act
      await _controller.Index();

      // assert
      _mockHomepageService.Verify(x => x.GetHomepageViewModelAsync(true), Times.Once);
    }

    [Test]
    public async Task should_pass_is_authenticated_false_when_user_is_anonymous()
    {
      // arrange
      var viewModel = new HomepageViewModel { IsAuthenticated = false };

      _mockHomepageService
        .Setup(x => x.GetHomepageViewModelAsync(false))
        .ReturnsAsync(viewModel);

      _controller = CreateController(isAuthenticated: false);

      // act
      await _controller.Index();

      // assert
      _mockHomepageService.Verify(x => x.GetHomepageViewModelAsync(false), Times.Once);
    }

    [Test]
    public async Task should_return_homepage_view_model_with_opportunity_cards()
    {
      // arrange
      var viewModel = new HomepageViewModel
      {
        IsAuthenticated = false,
        OpportunityCards = new List<OpportunityCardViewModel>
        {
          new OpportunityCardViewModel { ArtistName = "Test" },
        }.AsReadOnly(),
        FemaleArtists = new List<RandomFemaleArtistItemViewModel>(),
      };

      _mockHomepageService
        .Setup(x => x.GetHomepageViewModelAsync(false))
        .ReturnsAsync(viewModel);

      _controller = CreateController(isAuthenticated: false);

      // act
      var result = await _controller.Index();

      // assert
      var view = result.Should().BeOfType<ViewResult>().Subject;
      var model = view.Model.Should().BeOfType<HomepageViewModel>().Subject;
      model.OpportunityCards.Should().HaveCount(1);
    }

    [Test]
    public async Task should_return_homepage_view_model_with_female_artists()
    {
      // arrange
      var viewModel = new HomepageViewModel
      {
        IsAuthenticated = false,
        OpportunityCards = new List<OpportunityCardViewModel>().AsReadOnly(),
        FemaleArtists = new List<RandomFemaleArtistItemViewModel>
        {
          new RandomFemaleArtistItemViewModel { Name = "Gulistan" },
        },
      };

      _mockHomepageService
        .Setup(x => x.GetHomepageViewModelAsync(false))
        .ReturnsAsync(viewModel);

      _controller = CreateController(isAuthenticated: false);

      // act
      var result = await _controller.Index();

      // assert
      var view = result.Should().BeOfType<ViewResult>().Subject;
      var model = view.Model.Should().BeOfType<HomepageViewModel>().Subject;
      model.FemaleArtists.Should().HaveCount(1);
    }

    private HomeController CreateController(bool isAuthenticated)
    {
      var controller = new HomeController(_mockHomepageService.Object);

      var claims = new List<Claim>();

      if (isAuthenticated)
      {
        claims.Add(new Claim(ClaimTypes.Name, "testuser"));
      }

      var identity = new ClaimsIdentity(
        claims,
        isAuthenticated ? "TestAuth" : null);

      var principal = new ClaimsPrincipal(identity);

      controller.ControllerContext = new ControllerContext
      {
        HttpContext = new DefaultHttpContext { User = principal },
      };

      return controller;
    }
  }
}
