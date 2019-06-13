namespace Bejebeje.Api.Tests.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Api.Controllers;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Services.Services.Interfaces;
  using Bejebeje.ViewModels.Artist;
  using FluentAssertions;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class ArtistsControllerTests
  {
    private Mock<IArtistsService> artistsServiceMock;

    private Mock<ILogger<ArtistsController>> loggerMock;

    private ArtistsController artistsController;

    [SetUp]
    public void Setup()
    {
      artistsServiceMock = new Mock<IArtistsService>(MockBehavior.Strict);

      loggerMock = new Mock<ILogger<ArtistsController>>(MockBehavior.Loose);

      artistsController = new ArtistsController(
        artistsServiceMock.Object,
        loggerMock.Object);
    }

    [Test]
    public async Task GetArtists_WhenSearchParamIsNullAndWithNoArtistsFromTheService_ReturnsAnOkObjectResultWithEmptyList()
    {
      // arrange
      string searchParameter = null;
      int pageNumber = 1;
      int pageSize = 10;

      artistsServiceMock
        .Setup(x => x.GetArtistsAsync(pageNumber, pageSize))
        .ReturnsAsync(new List<ArtistCardViewModel>());

      artistsServiceMock
        .Setup(x => x.SearchArtistsAsync(searchParameter))
        .Verifiable();

      // act
      IActionResult result = await artistsController.GetArtists(searchParameter, pageNumber, pageSize);

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      List<ArtistCardViewModel> artists = okObjectResult.Value as List<ArtistCardViewModel>;

      artists.Should().BeEmpty();
      artistsServiceMock.Verify(x => x.GetArtistsAsync(pageNumber, pageSize), Times.Once);
      artistsServiceMock.Verify(x => x.SearchArtistsAsync(searchParameter), Times.Never);
    }

    [Test]
    public async Task GetArtists_WhenSearchParamIsEmptyStringAndWithNoArtistsFromTheService_ReturnsAnOkObjectResultWithEmptyList()
    {
      // arrange
      string searchParameter = string.Empty;
      int pageNumber = 1;
      int pageSize = 10;

      artistsServiceMock
        .Setup(x => x.GetArtistsAsync(pageNumber, pageSize))
        .ReturnsAsync(new List<ArtistCardViewModel>());

      artistsServiceMock
        .Setup(x => x.SearchArtistsAsync(searchParameter))
        .Verifiable();

      // act
      IActionResult result = await artistsController.GetArtists(searchParameter, pageNumber, pageSize);

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      List<ArtistCardViewModel> artists = okObjectResult.Value as List<ArtistCardViewModel>;

      artists.Should().BeEmpty();
      artistsServiceMock.Verify(x => x.GetArtistsAsync(pageNumber, pageSize), Times.Once);
      artistsServiceMock.Verify(x => x.SearchArtistsAsync(searchParameter), Times.Never);
    }

    [Test]
    public async Task GetArtists_WhenSearchParamIsNullAndWithArtistsFromTheService_ReturnsAnOkObjectResultWithArtists()
    {
      // arrange
      string searchParameter = null;
      string artistFirstName = "Johnny";
      string artistLastName = "Cash";
      int artistImageId = 1;
      string artistSlug = "johnny-cash";
      int pageNumber = 1;
      int pageSize = 10;

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
        .Setup(x => x.GetArtistsAsync(pageNumber, pageSize))
        .ReturnsAsync(artistsFromService);

      artistsServiceMock
        .Setup(x => x.SearchArtistsAsync(searchParameter))
        .Verifiable();

      // act
      IActionResult result = await artistsController.GetArtists(searchParameter, pageNumber, pageSize);

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
      artistsServiceMock.Verify(x => x.GetArtistsAsync(pageNumber, pageSize), Times.Once);
      artistsServiceMock.Verify(x => x.SearchArtistsAsync(searchParameter), Times.Never);
    }

    [Test]
    public async Task GetArtists_WhenSearchParamIsEmptyStringAndWithArtistsFromTheService_ReturnsAnOkObjectResultWithArtists()
    {
      // arrange
      string searchParameter = "";
      string artistFirstName = "Johnny";
      string artistLastName = "Cash";
      int artistImageId = 1;
      string artistSlug = "johnny-cash";
      int pageNumber = 1;
      int pageSize = 10;


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
        .Setup(x => x.GetArtistsAsync(pageNumber, pageSize))
        .ReturnsAsync(artistsFromService);

      artistsServiceMock
        .Setup(x => x.SearchArtistsAsync(searchParameter))
        .Verifiable();

      // act
      IActionResult result = await artistsController.GetArtists(searchParameter, pageNumber, pageSize);

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
      artistsServiceMock.Verify(x => x.GetArtistsAsync(pageNumber, pageSize), Times.Once);
      artistsServiceMock.Verify(x => x.SearchArtistsAsync(searchParameter), Times.Never);
    }

    [Test]
    public async Task GetArtists_WhenSearchParamExistsAndWithNoMatchedArtistsFromTheService_ReturnsAnOkObjectResultWithEmptyListOfArtistCardViewModels()
    {
      // arrange
      string searchParameter = "watson";
      int pageNumber = 1;
      int pageSize = 10;

      artistsServiceMock
        .Setup(x => x.GetArtistsAsync(pageNumber, pageSize))
        .Verifiable();

      artistsServiceMock
        .Setup(x => x.SearchArtistsAsync(searchParameter))
        .ReturnsAsync(new List<ArtistCardViewModel>());

      // act
      IActionResult result = await artistsController.GetArtists(searchParameter, pageNumber, pageSize);

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      List<ArtistCardViewModel> artists = okObjectResult.Value as List<ArtistCardViewModel>;

      artists.Should().BeEmpty();
      artistsServiceMock.Verify(x => x.GetArtistsAsync(pageNumber, pageSize), Times.Never);
      artistsServiceMock.Verify(x => x.SearchArtistsAsync(searchParameter), Times.Once);
    }

    [Test]
    public async Task GetArtists_WhenSearchParamExistsAndWithMatchedArtistsFromTheService_ReturnsAnOkObjectResultWithArtists()
    {
      // arrange
      string searchParameter = "johnny";
      string artistFirstName = "Johnny";
      string artistLastName = "Cash";
      int artistImageId = 1;
      string artistSlug = "johnny-cash";
      int pageNumber = 1;
      int pageSize = 10;

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
        .Setup(x => x.GetArtistsAsync(pageNumber, pageSize))
        .Verifiable();

      artistsServiceMock
        .Setup(x => x.SearchArtistsAsync(searchParameter))
        .ReturnsAsync(artistsFromService);

      // act
      IActionResult result = await artistsController.GetArtists(searchParameter, pageNumber, pageSize);

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
      artistsServiceMock.Verify(x => x.GetArtistsAsync(pageNumber, pageSize), Times.Never);
      artistsServiceMock.Verify(x => x.SearchArtistsAsync(searchParameter), Times.Once);
    }

    [Test]
    public async Task GetArtistDetailsAsync_WhenParamIsNull_ThrowsAnArgumentNullException()
    {
      // arrange
      string artistSlug = null;

      // act
      Func<Task> action = async () => await artistsController.GetArtistDetails(artistSlug);

      // assert
      await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task GetArtistDetailsAsync_WhenServiceThrowsAArtistNotFoundException_ReturnsANotFoundResult()
    {
      // arrange
      string artistSlug = "john-doe";

      artistsServiceMock
        .Setup(x => x.GetArtistDetailsAsync(artistSlug))
        .ThrowsAsync(new ArtistNotFoundException(artistSlug));

      // act
      IActionResult result = await artistsController.GetArtistDetails(artistSlug);

      // assert
      result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task GetArtistDetailsAsync_WhenServiceReturnsArtistDetails_ReturnsAOkObjectResultWithTheData()
    {
      // arrange
      string artistSlug = "fats-waller";
      int artistId = 1;
      string artistFirstName = "Fats";
      string artistLastName = "Waller";
      int artistImageId = 1;
      DateTime artistCreatedAt = DateTime.UtcNow;

      ArtistDetailsViewModel fatsWallerDetails = new ArtistDetailsViewModel
      {
        Id = 1,
        FirstName = artistFirstName,
        LastName = artistLastName,
        ImageId = artistImageId,
        CreatedAt = artistCreatedAt,
        Slug = artistSlug
      };

      artistsServiceMock
        .Setup(x => x.GetArtistDetailsAsync(artistSlug))
        .ReturnsAsync(fatsWallerDetails);

      // act
      IActionResult result = await artistsController.GetArtistDetails(artistSlug);

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      ArtistDetailsViewModel artistDetails = okObjectResult.Value as ArtistDetailsViewModel;

      artistDetails.Should().NotBeNull();
      artistDetails.Id.Should().Be(artistId);
      artistDetails.FirstName.Should().Be(artistFirstName);
      artistDetails.LastName.Should().Be(artistLastName);
      artistDetails.ImageId.Should().Be(artistImageId);
      artistDetails.Slug.Should().Be(artistSlug);
      artistDetails.CreatedAt.Should().Be(artistCreatedAt);
    }
  }
}
