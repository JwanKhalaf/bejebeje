namespace Bejebeje.Api.Tests.Controllers
{
  using System;
  using System.Threading.Tasks;
  using Bejebeje.Api.Controllers;
  using Bejebeje.Common.Exceptions;
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
  }
}
