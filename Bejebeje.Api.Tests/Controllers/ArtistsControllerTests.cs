namespace Bejebeje.Api.Tests.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Api.Controllers;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Models.Artist;
  using Bejebeje.Models.Paging;
  using Bejebeje.Services.Services.Interfaces;
  using FluentAssertions;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;
  using Moq;
  using NodaTime;
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
    public async Task SearchArtists_WhenSearchParamIsNullAndNoArtistsAreReturnedFromTheService_ReturnsAnOkObjectResultWithEmptyList()
    {
      // arrange
      string searchParameter = null;
      int offset = 0;
      int limit = 10;

      artistsServiceMock
        .Setup(x => x.SearchArtistsAsync(searchParameter, offset, limit))
        .ReturnsAsync(new PagedArtistSearchResponse());

      // act
      IActionResult result = await artistsController.SearchArtists(searchParameter, offset, limit);

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      PagedArtistSearchResponse responseModel = okObjectResult.Value as PagedArtistSearchResponse;

      responseModel.Artists.Should().BeEmpty();
      artistsServiceMock.Verify(x => x.SearchArtistsAsync(searchParameter, offset, limit), Times.Once);
    }

    [Test]
    public async Task SearchArtists_WhenSearchParamIsAnEmptyStringAndNoArtistsAreReturnedFromTheService_ReturnsAnOkObjectResultWithEmptyList()
    {
      // arrange
      string searchParameter = string.Empty;
      int offset = 0;
      int limit = 10;

      artistsServiceMock
        .Setup(x => x.SearchArtistsAsync(searchParameter, offset, limit))
        .ReturnsAsync(new PagedArtistSearchResponse());

      // act
      IActionResult result = await artistsController.SearchArtists(searchParameter);

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      PagedArtistSearchResponse responseModel = okObjectResult.Value as PagedArtistSearchResponse;

      responseModel.Artists.Should().BeEmpty();
      artistsServiceMock.Verify(x => x.SearchArtistsAsync(searchParameter, offset, limit), Times.Once);
    }

    [Test]
    public async Task SearchArtists_WhenSearchParamIsNullAndArtistsAreReturnedFromTheService_ReturnsAnOkObjectResultWithArtists()
    {
      // arrange
      string searchParameter = null;
      int offset = 0;
      int limit = 10;

      string artistFullName = "Johnny Cash";
      string artistSlug = "johnny-cash";
      bool artistHasImage = true;

      PagedArtistSearchResponse artistsResponseFromService = new PagedArtistSearchResponse
      {
        Artists = new List<ArtistSearchResponse>
        {
          new ArtistSearchResponse
          {
            FullName = artistFullName,
            HasImage = artistHasImage,
            PrimarySlug = artistSlug,
          },
        },
        Paging = new PagingResponse
        {
          Offset = offset,
          Limit = limit,
        },
      };

      artistsServiceMock
        .Setup(x => x.SearchArtistsAsync(searchParameter, offset, limit))
        .ReturnsAsync(artistsResponseFromService);

      // act
      IActionResult result = await artistsController.SearchArtists(searchParameter, offset, limit);

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      PagedArtistSearchResponse responseModel = okObjectResult.Value as PagedArtistSearchResponse;

      responseModel.Artists.Should().NotBeEmpty();
      responseModel.Artists.Should().HaveCount(1);

      ArtistSearchResponse artist = responseModel.Artists.First();

      artist.FullName.Should().Be(artistFullName);
      artist.HasImage.Should().BeTrue();
      artist.PrimarySlug.Should().Be(artistSlug);
      artistsServiceMock.Verify(x => x.SearchArtistsAsync(searchParameter, offset, limit), Times.Once);
    }

    [Test]
    public async Task SearchArtists_WhenSearchParamIsAnEmptyStringAndArtistsAreReturnedFromTheService_ReturnsAnOkObjectResultWithEmptyList()
    {
      // arrange
      string searchParameter = string.Empty;
      int offset = 0;
      int limit = 10;

      string artistFullName = "Johnny Cash";
      bool artistHasImage = true;
      string artistSlug = "johnny-cash";

      PagedArtistSearchResponse artistsResponseFromService = new PagedArtistSearchResponse
      {
        Artists = new List<ArtistSearchResponse>
        {
          new ArtistSearchResponse
          {
            FullName = artistFullName,
            HasImage = artistHasImage,
            PrimarySlug = artistSlug,
          },
        },
        Paging = new PagingResponse
        {
          Offset = offset,
          Limit = limit,
        },
      };

      artistsServiceMock
        .Setup(x => x.SearchArtistsAsync(searchParameter, offset, limit))
        .ReturnsAsync(artistsResponseFromService);

      // act
      IActionResult result = await artistsController.SearchArtists(searchParameter);

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      PagedArtistSearchResponse responseModel = okObjectResult.Value as PagedArtistSearchResponse;

      responseModel.Artists.Should().NotBeEmpty();
      responseModel.Artists.Should().HaveCount(1);

      ArtistSearchResponse artist = responseModel.Artists.First();

      artist.FullName.Should().Be(artistFullName);
      artist.HasImage.Should().BeTrue();
      artist.PrimarySlug.Should().Be(artistSlug);
      artistsServiceMock.Verify(x => x.SearchArtistsAsync(searchParameter, offset, limit), Times.Once);
    }

    [Test]
    public async Task SearchArtists_WhenSearchParamExistsAndWithNoMatchedArtistsFromTheService_ReturnsAnOkObjectResultWithAnEmptyPagedArtistsResponse()
    {
      // arrange
      string searchParameter = "watson";
      int offset = 0;
      int limit = 10;

      artistsServiceMock
        .Setup(x => x.SearchArtistsAsync(searchParameter, offset, limit))
        .ReturnsAsync(new PagedArtistSearchResponse());

      // act
      IActionResult result = await artistsController.SearchArtists(searchParameter);

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      PagedArtistSearchResponse artists = okObjectResult.Value as PagedArtistSearchResponse;

      artists.Artists.Should().BeEmpty();
      artistsServiceMock.Verify(x => x.SearchArtistsAsync(searchParameter, offset, limit), Times.Once);
    }

    [Test]
    public async Task SearchArtists_WhenSearchParamExistsAndWithMatchedArtistsFromTheService_ReturnsAnOkObjectResultWithArtists()
    {
      // arrange
      string searchParameter = "johnny";
      int offset = 0;
      int limit = 10;

      string artistFullName = "johnny cash";
      bool artistHasImage = true;
      string artistSlug = "johnny-cash";

      PagedArtistSearchResponse pagedArtistsResponse = new PagedArtistSearchResponse
      {
        Artists = new List<ArtistSearchResponse>
        {
          new ArtistSearchResponse
          {
            FullName = artistFullName,
            HasImage = artistHasImage,
            PrimarySlug = artistSlug,
          },
        },
        Paging = new PagingResponse
        {
          Offset = offset,
          Limit = limit,
        },
      };

      artistsServiceMock
        .Setup(x => x.SearchArtistsAsync(searchParameter, offset, limit))
        .ReturnsAsync(pagedArtistsResponse);

      // act
      IActionResult result = await artistsController.SearchArtists(searchParameter);

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      PagedArtistSearchResponse artistsResponse = okObjectResult.Value as PagedArtistSearchResponse;

      List<ArtistSearchResponse> artists = artistsResponse.Artists.ToList();

      artists.Should().NotBeEmpty();
      artists.Should().HaveCount(1);

      ArtistSearchResponse artist = artists.First();

      artist.FullName.Should().Be(artistFullName);
      artist.HasImage.Should().BeTrue();
      artist.PrimarySlug.Should().Be(artistSlug);
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

    [Test]
    public async Task AddNewArtist_WhenArtistAlreadyExists_ReturnsBadRequest()
    {
      // arrange
      string firstName = "James";
      string lastNames = "Brown";
      string slug = "james-brown";

      CreateNewArtistRequest request = new CreateNewArtistRequest
      {
        FirstName = firstName,
        LastName = lastNames,
      };

      artistsServiceMock
        .Setup(x => x.CreateNewArtistAsync(request))
        .ThrowsAsync(new ArtistExistsException(slug));

      // act
      IActionResult result = await artistsController.AddNewArtist(request);

      // assert
      result.Should().BeOfType<BadRequestResult>();
    }

    [Test]
    public async Task AddNewArtist_WhenArtistDoesNotAlreadyExists_ReturnsCreatedAtRequest()
    {
      // arrange
      string firstName = "James";
      string lastNames = "Brown";
      string slug = "james-brown";
      DateTime createdAt = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();

      CreateNewArtistRequest request = new CreateNewArtistRequest
      {
        FirstName = firstName,
        LastName = lastNames,
      };

      artistsServiceMock
        .Setup(x => x.CreateNewArtistAsync(request))
        .ReturnsAsync(new CreateNewArtistResponse
        {
          Slug = slug,
          CreatedAt = createdAt,
        });

      // act
      IActionResult result = await artistsController.AddNewArtist(request);

      // assert
      result.Should().BeOfType<CreatedAtActionResult>();

      CreatedAtActionResult createdAtActionResult = result as CreatedAtActionResult;

      createdAtActionResult.Should().NotBeNull();

      CreateNewArtistResponse responseModel = createdAtActionResult.Value as CreateNewArtistResponse;

      responseModel.Slug.Should().Be(slug);
      responseModel.CreatedAt.Should().Be(createdAt);
      artistsServiceMock.Verify(x => x.CreateNewArtistAsync(request), Times.Once);
    }
  }
}
