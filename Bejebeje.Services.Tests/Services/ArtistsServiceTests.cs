namespace Bejebeje.Services.Tests.Services
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Domain;
  using Bejebeje.Models.Artist;
  using Bejebeje.Services.Services;
  using Bejebeje.Services.Services.Interfaces;
  using Bejebeje.Services.Tests.Helpers;
  using FluentAssertions;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class ArtistsServiceTests : DatabaseTestBase
  {
    private Mock<IArtistSlugsService> artistSlugsServiceMock;

    private ArtistsService artistsService;

    [SetUp]
    public void Setup()
    {
      SetupDataContext();

      artistsService = new ArtistsService(
        artistSlugsServiceMock.Object,
        Context);
    }

    [Test]
    public async Task GetArtistIdAsync_WhenArtistDoesNotExist_ThrowsAnArtistNotFoundException()
    {
      // arrange
      string artistSlug = "john-doe";

      // act
      Func<Task> action = async () => await artistsService.GetArtistIdAsync(artistSlug);

      // assert
      await action.Should().ThrowAsync<ArtistNotFoundException>();
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
            IsPrimary = true,
          },
        },
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
      Func<Task> action = async () => await artistsService.GetArtistDetailsAsync(artistSlug);

      // assert
      await action.Should().ThrowAsync<ArtistNotFoundException>();
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
            IsPrimary = true,
          },
        },
      };

      Context.Artists.Add(fatsWaller);
      Context.SaveChanges();

      // act
      ArtistDetailsResponse result = await artistsService.GetArtistDetailsAsync(artistSlug);

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
            IsPrimary = true,
          },
        },
        Image = new ArtistImage
        {
          Data = imageBytes,
          CreatedAt = DateTime.UtcNow,
        },
      };

      Context.Artists.Add(fatsWaller);
      Context.SaveChanges();

      // act
      ArtistDetailsResponse result = await artistsService.GetArtistDetailsAsync(artistSlug);

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
      // arrange
      int offset = 0;
      int limit = 10;

      // act
      PagedArtistsResponse result = await artistsService.GetArtistsAsync(offset, limit);

      // assert
      result.Should().BeOfType<PagedArtistsResponse>();
      result.Artists.Should().BeEmpty();
      result.Paging.Offset.Should().Be(offset);
      result.Paging.Limit.Should().Be(limit);
    }

    [Test]
    public async Task GetArtistsAsync_WithData_ReturnsAListOfArtists()
    {
      // arrange
      string artistFirstName = "Johnny";
      string artistLastName = "Cash";
      string artistSlug = "johnny-cash";
      int expectedArtistImageId = 1;
      int offset = 0;
      int limit = 10;

      Artist artistFromDb = new Artist
      {
        FirstName = artistFirstName,
        LastName = artistLastName,
        Image = new ArtistImage
        {
          Data = new byte[10],
          CreatedAt = DateTime.UtcNow,
        },
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = artistSlug,
            IsPrimary = true,
            CreatedAt = DateTime.UtcNow,
          },
        },
      };

      Context.Artists.Add(artistFromDb);
      Context.SaveChanges();

      // act
      PagedArtistsResponse result = await artistsService.GetArtistsAsync(offset, limit);

      // assert
      result.Should().BeOfType<PagedArtistsResponse>();
      result.Artists.Should().HaveCount(1);
      result.Artists.First().FirstName.Should().Be(artistFirstName);
      result.Artists.First().LastName.Should().Be(artistLastName);
      result.Artists.First().ImageId.Should().Be(expectedArtistImageId);
      result.Artists.First().Slugs.Should().HaveCount(1);
      result.Artists.First().Slugs.First().Name.Should().Be(artistSlug);
      result.Artists.First().Slugs.First().IsPrimary.Should().BeTrue();
    }

    [Test]
    public async Task SearchArtistsAsync_WhenNoArtistsMatch_ReturnsAnEmptyListOfArtistCardViewModels()
    {
      // arrange
      string artistName = "Watson";
      int offset = 0;
      int limit = 10;

      string artistFirstName = "Fats";
      string artistLastName = "Waller";
      string artistFullName = "Fats Waller";
      string artistSlug = "fats-waller";
      DateTime artistCreatedAt = DateTime.UtcNow;

      string baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;

      string filePath = baseDirectoryPath + "/Assets/fats-waller.jpg";

      byte[] imageBytes = await File.ReadAllBytesAsync(filePath);

      Artist fatsWaller = new Artist
      {
        FirstName = artistFirstName,
        LastName = artistLastName,
        FullName = artistFullName,
        CreatedAt = artistCreatedAt,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = artistSlug,
            IsPrimary = true,
            CreatedAt = artistCreatedAt,
          },
        },
        Image = new ArtistImage
        {
          Data = imageBytes,
          CreatedAt = artistCreatedAt,
        },
      };

      Context.Artists.Add(fatsWaller);
      Context.SaveChanges();

      // act
      PagedArtistsResponse result = await artistsService.SearchArtistsAsync(artistName, offset, limit);

      // assert
      result.Should().NotBeNull();
      result.Should().BeOfType<PagedArtistsResponse>();
      result.Artists.Should().HaveCount(0);
    }

    [Test]
    public async Task SearchArtistsAsync_WhenThereIsAMatchOnFirstNameOnly_ReturnsAPopulatedListOfArtistCardViewModels()
    {
      // arrange
      string artistName = "fats";
      int offset = 0;
      int limit = 10;

      string artistFirstName = "Fats";
      string artistLastName = "Waller";
      string artistSlug = "something-different-from-first-and-last-name";
      DateTime artistCreatedAt = DateTime.UtcNow;

      string baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;

      string filePath = baseDirectoryPath + "/Assets/fats-waller.jpg";

      byte[] imageBytes = await File.ReadAllBytesAsync(filePath);

      Artist fatsWaller = new Artist
      {
        FirstName = artistFirstName,
        LastName = artistLastName,
        FullName = $"{artistFirstName} {artistLastName}",
        CreatedAt = artistCreatedAt,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = artistSlug,
            IsPrimary = true,
            CreatedAt = artistCreatedAt,
          },
        },
        Image = new ArtistImage
        {
          Data = imageBytes,
          CreatedAt = artistCreatedAt,
        },
      };

      Context.Artists.Add(fatsWaller);
      Context.SaveChanges();

      // act
      PagedArtistsResponse result = await artistsService.SearchArtistsAsync(artistName, offset, limit);

      // assert
      result.Should().NotBeNull();
      result.Should().BeOfType<PagedArtistsResponse>();

      ICollection<ArtistsResponse> artists = result.Artists;

      artists.Should().HaveCount(1);

      ArtistsResponse artist = artists.First();

      artist.FirstName.Should().Be(artistFirstName);
      artist.LastName.Should().Be(artistLastName);
      artist.Slugs.Should().HaveCount(1);
      artist.Slugs.First().Name.Should().Be(artistSlug);
      artist.Slugs.First().IsPrimary.Should().BeTrue();
      artist.ImageId.Should().Be(1);
    }

    [Test]
    public async Task SearchArtistsAsync_WhenThereIsAMatchOnLastNameOnly_ReturnsAPopulatedListOfArtistCardViewModels()
    {
      // arrange
      string artistName = "waller";
      int offset = 0;
      int limit = 10;

      string artistFirstName = "Fats";
      string artistLastName = "Waller";
      string artistSlug = "something-different-from-first-and-last-name";
      DateTime artistCreatedAt = DateTime.UtcNow;

      string baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;

      string filePath = baseDirectoryPath + "/Assets/fats-waller.jpg";

      byte[] imageBytes = await File.ReadAllBytesAsync(filePath);

      Artist fatsWaller = new Artist
      {
        FirstName = artistFirstName,
        LastName = artistLastName,
        FullName = $"{artistFirstName} {artistLastName}",
        CreatedAt = artistCreatedAt,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = artistSlug,
            IsPrimary = true,
            CreatedAt = artistCreatedAt,
          },
        },
        Image = new ArtistImage
        {
          Data = imageBytes,
          CreatedAt = artistCreatedAt,
        },
      };

      Context.Artists.Add(fatsWaller);
      Context.SaveChanges();

      // act
      PagedArtistsResponse result = await artistsService.SearchArtistsAsync(artistName, offset, limit);

      // assert
      result.Should().NotBeNull();
      result.Should().BeOfType<PagedArtistsResponse>();

      ICollection<ArtistsResponse> artists = result.Artists;

      artists.Should().HaveCount(1);

      ArtistsResponse artist = artists.First();

      artist.FirstName.Should().Be(artistFirstName);
      artist.LastName.Should().Be(artistLastName);
      artist.Slugs.Should().HaveCount(1);
      artist.Slugs.First().Name.Should().Be(artistSlug);
      artist.Slugs.First().IsPrimary.Should().BeTrue();
      artist.ImageId.Should().Be(1);
    }

    [Test]
    public async Task SearchArtistsAsync_WhenThereIsAMatchOnArtistSlugOnly_ReturnsAPopulatedListOfArtistCardViewModels()
    {
      // arrange
      string artistName = "nokia";
      int offset = 0;
      int limit = 10;

      string artistFirstName = "Fats";
      string artistLastName = "Waller";
      string artistSlug = "nokia";
      DateTime artistCreatedAt = DateTime.UtcNow;

      string baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;

      string filePath = baseDirectoryPath + "/Assets/fats-waller.jpg";

      byte[] imageBytes = await File.ReadAllBytesAsync(filePath);

      Artist fatsWaller = new Artist
      {
        FirstName = artistFirstName,
        LastName = artistLastName,
        FullName = $"{artistFirstName} {artistLastName}",
        CreatedAt = artistCreatedAt,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = artistSlug,
            IsPrimary = true,
            CreatedAt = artistCreatedAt,
          },
        },
        Image = new ArtistImage
        {
          Data = imageBytes,
          CreatedAt = artistCreatedAt,
        },
      };

      Context.Artists.Add(fatsWaller);
      Context.SaveChanges();

      // act
      PagedArtistsResponse result = await artistsService.SearchArtistsAsync(artistName, offset, limit);

      // assert
      result.Should().NotBeNull();
      result.Should().BeOfType<PagedArtistsResponse>();
      ICollection<ArtistsResponse> artists = result.Artists;

      artists.Should().HaveCount(1);

      ArtistsResponse artist = artists.First();

      artist.FirstName.Should().Be(artistFirstName);
      artist.LastName.Should().Be(artistLastName);
      artist.Slugs.Should().HaveCount(1);
      artist.Slugs.First().Name.Should().Be(artistSlug);
      artist.Slugs.First().IsPrimary.Should().BeTrue();
      artist.ImageId.Should().Be(1);
    }

    [Test]
    public async Task SearchArtistsAsync_WhenSearchParamHasASpaceInItAndThereIsAMatch_ReturnsAPopulatedListOfArtistCardViewModels()
    {
      // arrange
      string artistName = "fats wal";
      int offset = 0;
      int limit = 10;

      string artistFirstName = "Fats";
      string artistLastName = "Waller";
      string artistSlug = "fats-waller";
      DateTime artistCreatedAt = DateTime.UtcNow;

      string baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;

      string filePath = baseDirectoryPath + "/Assets/fats-waller.jpg";

      byte[] imageBytes = await File.ReadAllBytesAsync(filePath);

      Artist fatsWaller = new Artist
      {
        FirstName = artistFirstName,
        LastName = artistLastName,
        FullName = $"{artistFirstName} {artistLastName}",
        CreatedAt = artistCreatedAt,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = artistSlug,
            IsPrimary = true,
            CreatedAt = artistCreatedAt,
          },
        },
        Image = new ArtistImage
        {
          Data = imageBytes,
          CreatedAt = artistCreatedAt,
        },
      };

      Context.Artists.Add(fatsWaller);
      Context.SaveChanges();

      // act
      PagedArtistsResponse result = await artistsService.SearchArtistsAsync(artistName, offset, limit);

      // assert
      result.Should().NotBeNull();
      result.Should().BeOfType<PagedArtistsResponse>();
      ICollection<ArtistsResponse> artists = result.Artists;

      artists.Should().HaveCount(1);

      ArtistsResponse artist = artists.First();

      artist.FirstName.Should().Be(artistFirstName);
      artist.LastName.Should().Be(artistLastName);
      artist.Slugs.Should().HaveCount(1);
      artist.Slugs.First().Name.Should().Be(artistSlug);
      artist.Slugs.First().IsPrimary.Should().BeTrue();
      artist.ImageId.Should().Be(1);
    }
  }
}
