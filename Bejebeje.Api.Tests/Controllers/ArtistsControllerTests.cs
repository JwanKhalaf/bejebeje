namespace Bejebeje.Api.Tests.Controllers
{
  using System.Collections.Generic;
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
    public async Task Get_WithNoData_ReturnsAnOkObjectResult()
    {
      // arrange
      artistsServiceMock
        .Setup(x => x.GetArtistsAsync())
        .ReturnsAsync(new List<ArtistCardViewModel>());

      // act
      var result = await artistsController.Get();

      // assert
      result.Should().BeOfType<OkObjectResult>();
    }
  }
}
