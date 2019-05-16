namespace Bejebeje.Api.Tests.Controllers
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Api.Controllers;
  using Bejebeje.Services.Services.Interfaces;
  using Bejebeje.ViewModels.Artist;
  using FluentAssertions;
  using Microsoft.AspNetCore.Mvc;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class ArtistsControllerTests
  {
    private Mock<IArtistsService> artistsServiceMock;

    private ArtistsController artistsController;

    [SetUp]
    public void Setup()
    {
      artistsServiceMock = new Mock<IArtistsService>(MockBehavior.Strict);
      artistsController = new ArtistsController(artistsServiceMock.Object);
    }

    [Test]
    public async Task Get_WithNoArtistsFromTheService_ReturnsAnOkObjectResultWithEmptyList()
    {
      // arrange
      artistsServiceMock
        .Setup(x => x.GetArtistsAsync())
        .ReturnsAsync(new List<ArtistCardViewModel>());

      // act
      IActionResult result = await artistsController.Get();

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      List<ArtistCardViewModel> artists = okObjectResult.Value as List<ArtistCardViewModel>;

      artists.Should().BeEmpty();
    }

    [Test]
    public async Task Get_WithArtistsFromTheService_ReturnsAnOkObjectResultWithArtists()
    {
      // arrange
      string artistFirstName = "Johnny";
      string artistLastName = "Cash";
      int artistImageId = 1;
      string artistSlug = "johnny-cash";

      List<ArtistCardViewModel> artistsFromService = new List<ArtistCardViewModel>
      {
        new ArtistCardViewModel
        {
          FirstName = artistFirstName,
          LastName = artistLastName,
          ImageId = artistImageId,
          Slug = artistSlug
        }
      };

      artistsServiceMock
        .Setup(x => x.GetArtistsAsync())
        .ReturnsAsync(artistsFromService);

      // act
      IActionResult result = await artistsController.Get();

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      List<ArtistCardViewModel> artists = okObjectResult.Value as List<ArtistCardViewModel>;

      artists.Should().NotBeEmpty();
      artists.Should().HaveCount(1);
      artists.First().FirstName.Should().Be(artistFirstName);
      artists.First().LastName.Should().Be(artistLastName);
      artists.First().ImageId.Should().Be(artistImageId);
      artists.First().Slug.Should().Be(artistSlug);
    }

    [Test]
    public void FailingTest_ForTravisCI_Testing()
    {
      Assert.Fail();
    }
  }
}
