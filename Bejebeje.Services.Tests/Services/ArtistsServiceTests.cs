namespace Bejebeje.Services.Tests.Services
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Domain;
  using Bejebeje.Services.Services;
  using Bejebeje.Services.Tests.Helpers;
  using Bejebeje.ViewModels.Artist;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class ArtistsServiceTests : DatabaseTestBase
  {
    private ArtistsService artistsService;

    [SetUp]
    public void Setup()
    {
      SetupDataContext();

      artistsService = new ArtistsService(Context);
    }

    [Test]
    public async Task GetArtistIdAsync_WhenArtistDoesNotExist_ThrowsAnArtistNotFoundException()
    {
      // arrange
      string artistSlug = "john-doe";

      // act
      Func<Task> act = async () => await artistsService.GetArtistIdAsync(artistSlug);

      // assert
      await act.Should().ThrowAsync<ArtistNotFoundException>();
    }

    [Test]
    public async Task GetArtistIdAsync_WhenArtistDoesExist_ReturnsTheArtistId()
    {
      // arrange
      string artistSlug = "fats-waller";
      string artistFirstName = "Fats";
      string artistLastName = "Waller";
      int expectedArtistId = 1;

      Artist fatsWaller = new Artist
      {
        FirstName = artistFirstName,
        LastName = artistLastName,
        CreatedAt = DateTime.UtcNow,
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

      // act
      int result = await artistsService.GetArtistIdAsync(artistSlug);

      // assert
      result.Should().Be(expectedArtistId);
    }

    [Test]
    public async Task GetArtistDetailsAsync_WhenArtistDoesNotExist_ThrowsAnArtistNotFoundException()
    {
      // arrange
      string artistSlug = "john-doe";

      // act
      Func<Task> act = async () => await artistsService.GetArtistDetailsAsync(artistSlug);

      // assert
      await act.Should().ThrowAsync<ArtistNotFoundException>();
    }

    [Test]
    public async Task GetArtistDetailsAsync_WhenArtistDoesExistButHasNoImage_ReturnsArtistDetailsWithImageIdAsZero()
    {
      // arrange
      string artistSlug = "fats-waller";
      string artistFirstName = "Fats";
      string artistLastName = "Waller";
      int expectedArtistId = 1;
      int expectedImageId = 0;

      Artist fatsWaller = new Artist
      {
        FirstName = artistFirstName,
        LastName = artistLastName,
        CreatedAt = DateTime.UtcNow,
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

      // act
      ArtistDetailsViewModel result = await artistsService.GetArtistDetailsAsync(artistSlug);

      // assert
      result.Should().NotBeNull();
      result.Id.Should().Be(expectedArtistId);
      result.FirstName.Should().Be(artistFirstName);
      result.LastName.Should().Be(artistLastName);
      result.Slug.Should().Be(artistSlug);
      result.ImageId.Should().Be(expectedImageId);
    }

    [Test]
    public async Task GetArtistDetailsAsync_WhenArtistDoesExistAndHasAnImage_ReturnsArtistDetailsWithCorrectImageId()
    {
      // arrange
      string artistSlug = "fats-waller";
      string artistFirstName = "Fats";
      string artistLastName = "Waller";
      int expectedArtistId = 1;
      int expectedImageId = 1;

      string baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;

      string filePath = baseDirectoryPath + "/Assets/fats-waller.jpg";

      byte[] imageBytes = await File.ReadAllBytesAsync(filePath);

      Artist fatsWaller = new Artist
      {
        FirstName = artistFirstName,
        LastName = artistLastName,
        CreatedAt = DateTime.UtcNow,
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

      // act
      ArtistDetailsViewModel result = await artistsService.GetArtistDetailsAsync(artistSlug);

      // assert
      result.Should().NotBeNull();
      result.Id.Should().Be(expectedArtistId);
      result.FirstName.Should().Be(artistFirstName);
      result.LastName.Should().Be(artistLastName);
      result.Slug.Should().Be(artistSlug);
      result.ImageId.Should().Be(expectedImageId);
    }

    [Test]
    public async Task GetArtistsAsync_WithNoData_ReturnsAnEmptyListOfArtists()
    {
      // act
      IList<ArtistCardViewModel> result = await artistsService.GetArtistsAsync(string.Empty);

      // assert
      result.Should().BeOfType<List<ArtistCardViewModel>>();
      result.Should().BeEmpty();
    }

    [Test]
    public async Task GetArtistsAsync_WithData_ReturnsAListOfArtists()
    {
      // arrange
      string artistFirstName = "Johnny";
      string artistLastName = "Cash";
      string artistSlug = "johnny-cash";
      int expectedArtistImageId = 1;

      Artist artistFromDb = new Artist
      {
        FirstName = artistFirstName,
        LastName = artistLastName,
        Image = new ArtistImage
        {
          Data = new byte[10],
          CreatedAt = DateTime.UtcNow
        },
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = artistSlug,
            IsPrimary = true,
            CreatedAt = DateTime.UtcNow
          }
        }
      };

      Context.Artists.Add(artistFromDb);
      Context.SaveChanges();

      // act
      IList<ArtistCardViewModel> result = await artistsService.GetArtistsAsync(string.Empty);

      // assert
      result.Should().BeOfType<List<ArtistCardViewModel>>();
      result.Should().HaveCount(1);
      result.First().FirstName.Should().Be(artistFirstName);
      result.First().LastName.Should().Be(artistLastName);
      result.First().ImageId.Should().Be(expectedArtistImageId);
      result.First().Slug.Should().Be(artistSlug);
    }
  }
}
