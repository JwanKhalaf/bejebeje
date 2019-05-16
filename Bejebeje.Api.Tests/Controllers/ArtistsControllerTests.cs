namespace Bejebeje.Api.Tests.Controllers
{
  using Bejebeje.Api.Controllers;
  using Bejebeje.Services.Services.Interfaces;
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
    public void EnsureThatAnEmptyListIsReturnedWhenThereIsNoData()
    {

    }
  }
}
