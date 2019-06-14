namespace Bejebeje.Api.Tests.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Api.Controllers;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Models.Artist;
  using Bejebeje.Models.ArtistSlug;
  using Bejebeje.Models.Paging;
  using Bejebeje.Services.Services.Interfaces;
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
    public async Task GetArtists_WhenSearchParamIsNullAndNoArtistsAreReturnedFromTheService_ReturnsAnOkObjectResultWithEmptyList()
    {
      // arrange
      string searchParameter = null;
      int offset = 0;
      int limit = 10;

      artistsServiceMock
        .Setup(x => x.GetArtistsAsync(offset, limit))
        .ReturnsAsync(new PagedArtistsResponse());

      artistsServiceMock
        .Setup(x => x.SearchArtistsAsync(searchParameter, offset, limit))
        .Verifiable();

      // act
      IActionResult result = await artistsController.GetArtists(searchParameter, offset, limit);

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      PagedArtistsResponse responseModel = okObjectResult.Value as PagedArtistsResponse;

      responseModel.Artists.Should().BeEmpty();
      artistsServiceMock.Verify(x => x.GetArtistsAsync(offset, limit), Times.Once);
      artistsServiceMock.Verify(x => x.SearchArtistsAsync(searchParameter, offset, limit), Times.Never);
    }

    [Test]
    public async Task GetArtists_WhenSearchParamIsAnEmptyStringAndNoArtistsAreReturnedFromTheService_ReturnsAnOkObjectResultWithEmptyList()
    {
      // arrange
      string searchParameter = string.Empty;
      int offset = 0;
      int limit = 10;

      artistsServiceMock
        .Setup(x => x.GetArtistsAsync(offset, limit))
        .ReturnsAsync(new PagedArtistsResponse());

      artistsServiceMock
        .Setup(x => x.SearchArtistsAsync(searchParameter, offset, limit))
        .Verifiable();

      // act
      IActionResult result = await artistsController.GetArtists(searchParameter);

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      PagedArtistsResponse responseModel = okObjectResult.Value as PagedArtistsResponse;

      responseModel.Artists.Should().BeEmpty();
      artistsServiceMock.Verify(x => x.GetArtistsAsync(offset, limit), Times.Once);
      artistsServiceMock.Verify(x => x.SearchArtistsAsync(searchParameter, offset, limit), Times.Never);
    }

    [Test]
    public async Task GetArtists_WhenSearchParamIsNullAndArtistsAreReturnedFromTheService_ReturnsAnOkObjectResultWithArtists()
    {
      // arrange
      string searchParameter = null;
      string artistFirstName = "Johnny";
      string artistLastName = "Cash";
      int artistImageId = 1;
      string artistSlug = "johnny-cash";
      int offset = 0;
      int limit = 10;

      PagedArtistsResponse artistsResponseFromService = new PagedArtistsResponse
      {
        Artists = new List<ArtistsResponse>
        {
          new ArtistsResponse
          {
            FirstName = artistFirstName,
            LastName = artistLastName,
            ImageId = artistImageId,
            Slugs = new List<ArtistSlugResponse>
            {
              new ArtistSlugResponse
              {
                Name = artistSlug,
                IsPrimary = true,
              },
            },
          },
        },
        Paging = new PagingResponse
        {
          Offset = offset,
          Limit = limit,
        },
      };

      artistsServiceMock
        .Setup(x => x.GetArtistsAsync(offset, limit))
        .ReturnsAsync(artistsResponseFromService);

      artistsServiceMock
        .Setup(x => x.SearchArtistsAsync(searchParameter, offset, limit))
        .Verifiable();

      // act
      IActionResult result = await artistsController.GetArtists(searchParameter, offset, limit);

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      PagedArtistsResponse responseModel = okObjectResult.Value as PagedArtistsResponse;

      responseModel.Artists.Should().NotBeEmpty();
      responseModel.Artists.Should().HaveCount(1);

      ArtistsResponse artist = responseModel.Artists.First();

      artist.FirstName.Should().Be(artistFirstName);
      artist.LastName.Should().Be(artistLastName);
      artist.ImageId.Should().Be(artistImageId);
      artist.Slugs.Should().HaveCount(1);
      artist.Slugs.First().Name.Should().Be(artistSlug);
      artist.Slugs.First().IsPrimary.Should().BeTrue();
      artistsServiceMock.Verify(x => x.GetArtistsAsync(offset, limit), Times.Once);
      artistsServiceMock.Verify(x => x.SearchArtistsAsync(searchParameter, offset, limit), Times.Never);
    }

    [Test]
    public async Task GetArtists_WhenSearchParamIsAnEmptyStringAndArtistsAreReturnedFromTheService_ReturnsAnOkObjectResultWithEmptyList()
    {
      // arrange
      string searchParameter = string.Empty;
      string artistFirstName = "Johnny";
      string artistLastName = "Cash";
      int artistImageId = 1;
      string artistSlug = "johnny-cash";
      int offset = 0;
      int limit = 10;

      PagedArtistsResponse artistsResponseFromService = new PagedArtistsResponse
      {
        Artists = new List<ArtistsResponse>
        {
          new ArtistsResponse
          {
            FirstName = artistFirstName,
            LastName = artistLastName,
            ImageId = artistImageId,
            Slugs = new List<ArtistSlugResponse>
            {
              new ArtistSlugResponse
              {
                Name = artistSlug,
                IsPrimary = true,
              },
            },
          },
        },
        Paging = new PagingResponse
        {
          Offset = offset,
          Limit = limit,
        },
      };

      artistsServiceMock
        .Setup(x => x.GetArtistsAsync(offset, limit))
        .ReturnsAsync(artistsResponseFromService);

      artistsServiceMock
        .Setup(x => x.SearchArtistsAsync(searchParameter, offset, limit))
        .Verifiable();

      // act
      IActionResult result = await artistsController.GetArtists(searchParameter);

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      PagedArtistsResponse responseModel = okObjectResult.Value as PagedArtistsResponse;

      responseModel.Artists.Should().NotBeEmpty();
      responseModel.Artists.Should().HaveCount(1);

      ArtistsResponse artist = responseModel.Artists.First();

      artist.FirstName.Should().Be(artistFirstName);
      artist.LastName.Should().Be(artistLastName);
      artist.ImageId.Should().Be(artistImageId);
      artist.Slugs.Should().HaveCount(1);
      artist.Slugs.First().Name.Should().Be(artistSlug);
      artist.Slugs.First().IsPrimary.Should().BeTrue();
      artistsServiceMock.Verify(x => x.GetArtistsAsync(offset, limit), Times.Once);
      artistsServiceMock.Verify(x => x.SearchArtistsAsync(searchParameter, offset, limit), Times.Never);
    }

    [Test]
    public async Task GetArtists_WhenSearchParamExistsAndWithNoMatchedArtistsFromTheService_ReturnsAnOkObjectResultWithAnEmptyPagedArtistsResponse()
    {
      // arrange
      string searchParameter = "watson";
      int offset = 0;
      int limit = 10;

      artistsServiceMock
        .Setup(x => x.GetArtistsAsync(offset, limit))
        .Verifiable();

      artistsServiceMock
        .Setup(x => x.SearchArtistsAsync(searchParameter, offset, limit))
        .ReturnsAsync(new PagedArtistsResponse());

      // act
      IActionResult result = await artistsController.GetArtists(searchParameter);

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      PagedArtistsResponse artists = okObjectResult.Value as PagedArtistsResponse;

      artists.Artists.Should().BeEmpty();
      artistsServiceMock.Verify(x => x.GetArtistsAsync(offset, limit), Times.Never);
      artistsServiceMock.Verify(x => x.SearchArtistsAsync(searchParameter, offset, limit), Times.Once);
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
      int offset = 0;
      int limit = 10;

      PagedArtistsResponse pagedArtistsResponse = new PagedArtistsResponse
      {
        Artists = new List<ArtistsResponse>
        {
          new ArtistsResponse
          {
            FirstName = artistFirstName,
            LastName = artistLastName,
            ImageId = artistImageId,
            Slugs = new List<ArtistSlugResponse>
            {
              new ArtistSlugResponse
              {
                Name = artistSlug,
                IsPrimary = true,
              },
            },
          },
        },
        Paging = new PagingResponse
        {
          Offset = offset,
          Limit = limit,
        },
      };

      artistsServiceMock
        .Setup(x => x.GetArtistsAsync(offset, limit))
        .Verifiable();

      artistsServiceMock
        .Setup(x => x.SearchArtistsAsync(searchParameter, offset, limit))
        .ReturnsAsync(pagedArtistsResponse);

      // act
      IActionResult result = await artistsController.GetArtists(searchParameter);

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      PagedArtistsResponse artistsResponse = okObjectResult.Value as PagedArtistsResponse;

      List<ArtistsResponse> artists = artistsResponse.Artists.ToList();

      artists.Should().NotBeEmpty();
      artists.Should().HaveCount(1);

      ArtistsResponse artist = artists.First();

      artist.FirstName.Should().Be(artistFirstName);
      artist.LastName.Should().Be(artistLastName);
      artist.ImageId.Should().Be(artistImageId);
      artist.Slugs.Should().HaveCount(1);
      artist.Slugs.First().Name.Should().Be(artistSlug);
      artist.Slugs.First().IsPrimary.Should().BeTrue();
      artistsServiceMock.Verify(x => x.GetArtistsAsync(offset, limit), Times.Never);
      artistsServiceMock.Verify(x => x.SearchArtistsAsync(searchParameter, offset, limit), Times.Once);
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

      ArtistDetailsResponse fatsWallerDetails = new ArtistDetailsResponse
      {
        Id = 1,
        FirstName = artistFirstName,
        LastName = artistLastName,
        ImageId = artistImageId,
        CreatedAt = artistCreatedAt,
        Slug = artistSlug,
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

      ArtistDetailsResponse artistDetails = okObjectResult.Value as ArtistDetailsResponse;

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
