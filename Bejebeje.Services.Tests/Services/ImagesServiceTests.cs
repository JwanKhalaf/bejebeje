namespace Bejebeje.Services.Tests.Services
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Domain;
  using Bejebeje.Services.Services;
  using Bejebeje.Services.Services.Interfaces;
  using Bejebeje.Services.Tests.Helpers;
  using FluentAssertions;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class ImagesServiceTests : DatabaseTestBase
  {
    private Mock<IArtistsService> artistsServiceMock;

    private ImagesService imagesService;

    [SetUp]
    public void Setup()
    {
      SetupDataContext();

      artistsServiceMock = new Mock<IArtistsService>(MockBehavior.Strict);

      imagesService = new ImagesService(artistsServiceMock.Object, Context);
    }

    [Test]
    public async Task GetArtistImageBytesAsync_WhenArtistDoesNotExist_ThrowsAnArtistNotFoundException()
    {
      // arrange
      string artistSlug = "john-doe";

      artistsServiceMock
        .Setup(x => x.GetArtistIdAsync(artistSlug))
        .ThrowsAsync(new ArtistNotFoundException(artistSlug));

      // act
      Func<Task> act = async () => await imagesService.GetArtistImageBytesAsync(artistSlug);

      // assert
      await act.Should().ThrowAsync<ArtistNotFoundException>();
    }

    [Test]
    public async Task GetArtistImageBytesAsync_WhenArtistHasNoImage_ThrowsAMissingArtistImageException()
    {
      // arrange
      int artistId = 1;
      string artistSlug = "fats-waller";
      string artistFirstName = "Fats";
      string artistLastName = "Waller";

      Artist fatsWaller = new Artist
      {
        Id = artistId,
        FirstName = artistFirstName,
        LastName = artistLastName,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = artistSlug,
            CreatedAt = DateTime.UtcNow,
            IsPrimary = true
          }
        }
      };

      Context.Artists.Add(fatsWaller);
      Context.SaveChanges();

      artistsServiceMock
        .Setup(x => x.GetArtistIdAsync(artistSlug))
        .ReturnsAsync(1);

      // act
      Func<Task> act = async () => await imagesService.GetArtistImageBytesAsync(artistSlug);

      // assert
      await act.Should().ThrowAsync<MissingArtistImageException>();
    }

    [Test]
    public async Task GetArtistImageBytesAsync_WhenArtistHasAnImage_ReturnsBytesOfTheImage()
    {
      // arrange
      int artistId = 1;
      string artistSlug = "fats-waller";
      string artistFirstName = "Fats";
      string artistLastName = "Waller";

      string baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;

      string filePath = baseDirectoryPath + "/Assets/fats-waller.jpg";

      byte[] imageBytes = await File.ReadAllBytesAsync(filePath);

      Artist fatsWaller = new Artist
      {
        Id = artistId,
        FirstName = artistFirstName,
        LastName = artistLastName,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = artistSlug,
            CreatedAt = DateTime.UtcNow,
            IsPrimary = true
          }
        },
        Image = new ArtistImage
        {
          Data = imageBytes,
          CreatedAt = DateTime.UtcNow
        }
      };

      Context.Artists.Add(fatsWaller);
      Context.SaveChanges();

      artistsServiceMock
        .Setup(x => x.GetArtistIdAsync(artistSlug))
        .ReturnsAsync(1);

      // act
      byte[] result = await imagesService.GetArtistImageBytesAsync(artistSlug);

      // assert
      result.Should().NotBeNull();
      result.Should().BeEquivalentTo(imageBytes);
    }
  }
}
