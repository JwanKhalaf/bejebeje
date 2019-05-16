using System;
using System.IO;
using System.Threading.Tasks;
using Bejebeje.Api.Controllers;
using Bejebeje.Common.Exceptions;
using Bejebeje.Services.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Bejebeje.Api.Tests.Controllers
{
  [TestFixture]
  public class ArtistImageControllerTests
  {
    private Mock<IImagesService> imagesServiceMock;

    private Mock<ILogger<ArtistImagesController>> loggerMock;

    private ArtistImagesController artistImagesController;

    [SetUp]
    public void Setup()
    {
      imagesServiceMock = new Mock<IImagesService>(MockBehavior.Strict);

      loggerMock = new Mock<ILogger<ArtistImagesController>>(MockBehavior.Loose);

      artistImagesController = new ArtistImagesController(
        imagesServiceMock.Object,
        loggerMock.Object);
    }

    [Test]
    public async Task Get_WhenParamIsNull_ThrowsAnArgumentNullException()
    {
      // arrange
      string artistSlug = null;

      // act
      Func<Task> act = async () => await artistImagesController.Get(artistSlug);

      // assert
      await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Get_WhenArtistDoesNotExist_ReturnsANotFoundResult()
    {
      // arrange
      string artistSlug = "John Doe";

      imagesServiceMock
        .Setup(x => x.GetArtistImageBytesAsync(artistSlug))
        .ThrowsAsync(new ArtistNotFoundException(artistSlug));

      // act
      IActionResult result = await artistImagesController.Get(artistSlug);

      // assert
      result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task Get_WhenArtistExists_ReturnsTheImageFile()
    {
      // arrange
      string expectedFileContentType = "image/jpeg";
      string artistSlug = "acdc";

      string baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;

      string filePath = baseDirectoryPath + "/Assets/acdc.jpg";

      byte[] imageBytes = await File.ReadAllBytesAsync(filePath);

      imagesServiceMock
        .Setup(x => x.GetArtistImageBytesAsync(artistSlug))
        .ReturnsAsync(imageBytes);

      // act
      IActionResult result = await artistImagesController.Get(artistSlug);

      // assert
      result.Should().BeOfType<FileContentResult>();

      FileContentResult fileContentResult = result as FileContentResult;

      fileContentResult.ContentType.Should().Be(expectedFileContentType);

      fileContentResult.FileContents.Should().BeEquivalentTo(imageBytes);
    }
  }
}
