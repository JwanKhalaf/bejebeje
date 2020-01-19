namespace Bejebeje.Api.Tests.Controllers
{
  using System;
  using System.Threading.Tasks;
  using Bejebeje.Api.Controllers;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Models.Author;
  using Bejebeje.Services.Services.Interfaces;
  using FluentAssertions;
  using Microsoft.AspNetCore.Mvc;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class AuthorControllerTests
  {
    private Mock<IAuthorService> authorServiceMock;

    private AuthorController authorController;

    [SetUp]
    public void Setup()
    {
      authorServiceMock = new Mock<IAuthorService>(MockBehavior.Strict);

      authorController = new AuthorController(authorServiceMock.Object);
    }

    [Test]
    public async Task GetAuthorDetails_WhenAuthorSlugIsNull_ThrowsArgumentNullException()
    {
      // arrange
      string authorSlug = null;

      // act
      Func<Task> action = async () => await authorController.GetAuthorDetails(authorSlug);

      // assert
      await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task GetAuthorDetails_WhenAuthorSlugIsEmpty_ThrowsArgumentNullException()
    {
      // arrange
      string authorSlug = string.Empty;

      // act
      Func<Task> action = async () => await authorController.GetAuthorDetails(authorSlug);

      // assert
      await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task GetAuthorDetails_WhenAuthorDoesNotExist_ReturnsNotFound()
    {
      // arrange
      string authorSlug = "mr-potato";

      authorServiceMock
        .Setup(x => x.GetAuthorDetailsAsync(authorSlug))
        .ThrowsAsync(new AuthorNotFoundException(authorSlug));

      // act
      IActionResult result = await authorController.GetAuthorDetails(authorSlug);

      // assert
      result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task GetAuthorDetails_WhenAuthorExists_ReturnsOkResponseWithCorrectData()
    {
      // arrange
      string authorSlug = "acdc";

      string authorFirstName = "AC/DC";
      string authorBiography = "Awesome biography.";
      int authorImageId = 1;
      DateTime authorCreatedAt = new DateTime(2019, 07, 04, 12, 0, 0, DateTimeKind.Utc);

      authorServiceMock
        .Setup(x => x.GetAuthorDetailsAsync(authorSlug))
        .ReturnsAsync(new AuthorDetailsResponse
        {
          FirstName = authorFirstName,
          Slug = authorSlug,
          Biography = authorBiography,
          CreatedAt = authorCreatedAt,
          ImageId = authorImageId,
        });

      // act
      IActionResult result = await authorController.GetAuthorDetails(authorSlug);

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      AuthorDetailsResponse authorDetails = okObjectResult.Value as AuthorDetailsResponse;

      authorDetails.Should().NotBeNull();
      authorDetails.FirstName.Should().Be(authorFirstName);
      authorDetails.LastName.Should().Be(null);
      authorDetails.ImageId.Should().Be(authorImageId);
      authorDetails.Slug.Should().Be(authorSlug);
      authorDetails.CreatedAt.Should().Be(authorCreatedAt);
    }
  }
}
