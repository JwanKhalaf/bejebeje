namespace Bejebeje.Mvc.Tests.Contracts
{
  using System.Threading.Tasks;
  using Bejebeje.Models.Homepage;
  using Bejebeje.Services.Services.Interfaces;
  using FluentAssertions;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class IHomepageServiceTests
  {
    [Test]
    public async Task should_define_get_homepage_view_model_async_method()
    {
      // arrange
      var expectedViewModel = new HomepageViewModel
      {
        IsAuthenticated = true,
      };

      var mock = new Mock<IHomepageService>();

      mock
        .Setup(x => x.GetHomepageViewModelAsync(true))
        .ReturnsAsync(expectedViewModel);

      // act
      var result = await mock.Object.GetHomepageViewModelAsync(true);

      // assert
      result.Should().NotBeNull();
      result.IsAuthenticated.Should().BeTrue();
    }

    [Test]
    public async Task should_accept_false_for_anonymous_users()
    {
      // arrange
      var expectedViewModel = new HomepageViewModel
      {
        IsAuthenticated = false,
      };

      var mock = new Mock<IHomepageService>();

      mock
        .Setup(x => x.GetHomepageViewModelAsync(false))
        .ReturnsAsync(expectedViewModel);

      // act
      var result = await mock.Object.GetHomepageViewModelAsync(false);

      // assert
      result.Should().NotBeNull();
      result.IsAuthenticated.Should().BeFalse();
    }

    [Test]
    public async Task should_return_homepage_view_model_type()
    {
      // arrange
      var mock = new Mock<IHomepageService>();

      mock
        .Setup(x => x.GetHomepageViewModelAsync(It.IsAny<bool>()))
        .ReturnsAsync(new HomepageViewModel());

      // act
      var result = await mock.Object.GetHomepageViewModelAsync(false);

      // assert
      result.Should().BeOfType<HomepageViewModel>();
    }
  }
}
