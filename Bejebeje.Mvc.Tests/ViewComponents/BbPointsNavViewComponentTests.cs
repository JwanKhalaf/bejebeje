namespace Bejebeje.Mvc.Tests.ViewComponents
{
  using System;
  using System.Collections.Generic;
  using System.Security.Claims;
  using System.Threading.Tasks;
  using Bejebeje.Models.BbPoints;
  using Bejebeje.Services.Services.Interfaces;
  using FluentAssertions;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Mvc.Rendering;
  using Microsoft.AspNetCore.Mvc.ViewComponents;
  using Microsoft.Extensions.Logging;
  using Moq;
  using Mvc.ViewComponents;
  using NUnit.Framework;

  [TestFixture]
  public class BbPointsNavViewComponentTests
  {
    private Mock<IBbPointsService> _mockPointsService;
    private Mock<ILogger<BbPointsNavViewComponent>> _mockLogger;
    private BbPointsNavViewComponent _component;

    [SetUp]
    public void SetUp()
    {
      _mockPointsService = new Mock<IBbPointsService>();
      _mockLogger = new Mock<ILogger<BbPointsNavViewComponent>>();
      _component = new BbPointsNavViewComponent(_mockPointsService.Object, _mockLogger.Object);
    }

    [Test]
    public async Task should_return_empty_content_when_user_not_authenticated()
    {
      // arrange
      var identity = new ClaimsIdentity(); // not authenticated (no auth type)
      var principal = new ClaimsPrincipal(identity);
      SetupViewComponentContext(principal);

      // act
      var result = await _component.InvokeAsync();

      // assert
      result.Should().BeOfType<ContentViewComponentResult>();
      var content = result as ContentViewComponentResult;
      content.Content.Should().BeEmpty();
    }

    [Test]
    public async Task should_return_view_with_model_when_user_authenticated()
    {
      // arrange
      var claims = new List<Claim> { new Claim("sub", "user-123") };
      var principal = CreateAuthenticatedPrincipal(claims);
      SetupViewComponentContext(principal);

      var expectedModel = new NavBarPointsViewModel
      {
        TotalPoints = 42,
        ContributorLabel = "Contributor",
        HasPointsChanged = true,
      };

      _mockPointsService
        .Setup(s => s.GetNavBarDataAsync("user-123"))
        .ReturnsAsync(expectedModel);

      // act
      var result = await _component.InvokeAsync();

      // assert
      var viewResult = result.Should().BeOfType<ViewViewComponentResult>().Subject;
      var model = viewResult.ViewData.Model.Should().BeOfType<NavBarPointsViewModel>().Subject;
      model.TotalPoints.Should().Be(42);
      model.ContributorLabel.Should().Be("Contributor");
      model.HasPointsChanged.Should().BeTrue();
    }

    [Test]
    public async Task should_return_empty_content_when_sub_claim_missing()
    {
      // arrange
      var claims = new List<Claim> { new Claim("email", "test@test.com") };
      var principal = CreateAuthenticatedPrincipal(claims);
      SetupViewComponentContext(principal);

      // act
      var result = await _component.InvokeAsync();

      // assert
      result.Should().BeOfType<ContentViewComponentResult>();
    }

    [Test]
    public async Task should_return_empty_content_when_service_throws()
    {
      // arrange
      var claims = new List<Claim> { new Claim("sub", "user-456") };
      var principal = CreateAuthenticatedPrincipal(claims);
      SetupViewComponentContext(principal);

      _mockPointsService
        .Setup(s => s.GetNavBarDataAsync("user-456"))
        .ThrowsAsync(new Exception("database error"));

      // act
      var result = await _component.InvokeAsync();

      // assert
      result.Should().BeOfType<ContentViewComponentResult>();
      var content = result as ContentViewComponentResult;
      content.Content.Should().BeEmpty();
    }

    [Test]
    public async Task should_call_service_with_correct_user_id()
    {
      // arrange
      var claims = new List<Claim> { new Claim("sub", "specific-user-id") };
      var principal = CreateAuthenticatedPrincipal(claims);
      SetupViewComponentContext(principal);

      _mockPointsService
        .Setup(s => s.GetNavBarDataAsync("specific-user-id"))
        .ReturnsAsync(new NavBarPointsViewModel());

      // act
      await _component.InvokeAsync();

      // assert
      _mockPointsService.Verify(s => s.GetNavBarDataAsync("specific-user-id"), Times.Once);
    }

    private static ClaimsPrincipal CreateAuthenticatedPrincipal(List<Claim> claims)
    {
      var identity = new ClaimsIdentity(claims, "TestAuth");
      return new ClaimsPrincipal(identity);
    }

    private void SetupViewComponentContext(ClaimsPrincipal principal)
    {
      var httpContext = new DefaultHttpContext { User = principal };
      var viewContext = new ViewContext { HttpContext = httpContext };
      _component.ViewComponentContext = new ViewComponentContext { ViewContext = viewContext };
    }
  }
}
