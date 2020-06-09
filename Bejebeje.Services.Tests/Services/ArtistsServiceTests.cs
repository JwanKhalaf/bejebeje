namespace Bejebeje.Services.Tests.Services
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
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
  using NodaTime;
  using NUnit.Framework;

  [TestFixture]
  public class ArtistsServiceTests : DatabaseTestBase
  {
    private Mock<IArtistSlugsService> artistSlugsServiceMock;

    private ArtistsService artistsService;

    private TextInfo textInfo = new CultureInfo("ku-TR", false).TextInfo;

    [SetUp]
    public void Setup()
    {
      SetupDataContext();

      artistSlugsServiceMock = new Mock<IArtistSlugsService>(MockBehavior.Strict);

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
      string artistFirstName = "fats";
      string artistLastName = "waller";
      int expectedArtistId = 1;
      bool isDeleted = false;
      bool isApproved = true;

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
        IsDeleted = isDeleted,
        IsApproved = isApproved,
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
      string artistFirstName = "fats";
      string artistLastName = "waller";
      string artistFullName = $"{artistFirstName} {artistLastName}";
      bool isDeleted = false;
      bool isApproved = true;

      int expectedArtistId = 1;
      string expectedArtistFirstName = textInfo.ToTitleCase(artistFirstName);
      string expectedArtistLastName = textInfo.ToTitleCase(artistLastName);
      string expectedArtistFullName = textInfo.ToTitleCase(artistFullName);

      string baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;

      string filePath = baseDirectoryPath + "/Assets/fats-waller.jpg";

      byte[] imageBytes = await File.ReadAllBytesAsync(filePath);

      Artist fatsWaller = new Artist
      {
        FirstName = artistFirstName,
        LastName = artistLastName,
        FullName = artistFullName,
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
        IsDeleted = isDeleted,
        IsApproved = isApproved,
      };

      Context.Artists.Add(fatsWaller);
      Context.SaveChanges();

      // act
      GetArtistResponse result = await artistsService.GetArtistDetailsAsync(artistSlug);

      // assert
      result.Should().NotBeNull();
      result.Id.Should().Be(expectedArtistId);
      result.FirstName.Should().Be(expectedArtistFirstName);
      result.LastName.Should().Be(expectedArtistLastName);
      result.FullName.Should().Be(expectedArtistFullName);
      result.PrimarySlug.Should().Be(artistSlug);
      result.HasImage.Should().BeTrue();
    }

    [Test]
    public async Task GetArtistDetailsAsync_WhenArtistDoesExistAndHasAnImage_ReturnsArtistDetailsWithCorrectImageId()
    {
      // arrange
      string artistSlug = "fats-waller";
      string artistFirstName = "fats";
      string artistLastName = "waller";
      string artistFullName = $"{artistFirstName} {artistLastName}";
      bool isDeleted = false;
      bool isApproved = true;

      int expectedArtistId = 1;
      string expectedArtistFirstName = textInfo.ToTitleCase(artistFirstName);
      string expectedArtistLastName = textInfo.ToTitleCase(artistLastName);
      string expectedArtistFullName = textInfo.ToTitleCase(artistFullName);

      string baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;

      string filePath = baseDirectoryPath + "/Assets/fats-waller.jpg";

      byte[] imageBytes = await File.ReadAllBytesAsync(filePath);

      Artist fatsWaller = new Artist
      {
        FirstName = artistFirstName,
        LastName = artistLastName,
        FullName = artistFullName,
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
        IsDeleted = isDeleted,
        IsApproved = isApproved,
      };

      Context.Artists.Add(fatsWaller);
      Context.SaveChanges();

      // act
      GetArtistResponse result = await artistsService.GetArtistDetailsAsync(artistSlug);

      // assert
      result.Should().NotBeNull();
      result.Id.Should().Be(expectedArtistId);
      result.FirstName.Should().Be(expectedArtistFirstName);
      result.LastName.Should().Be(expectedArtistLastName);
      result.FullName.Should().Be(expectedArtistFullName);
      result.PrimarySlug.Should().Be(artistSlug);
      result.HasImage.Should().BeTrue();
    }

    [Test]
    public async Task CreateNewArtistAsync_WhenArtistAlreadyExists_ThrowsArtistExistsException()
    {
      // arrange
      string firstName = "Queen";
      string lastName = string.Empty;
      string fullName = "Queen";
      string artistSlug = "queen";

      artistSlugsServiceMock
        .Setup(x => x.GetArtistSlug(fullName))
        .Returns(artistSlug);

      Artist queen = new Artist
      {
        FirstName = "Queen",
        FullName = "Queen",
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = artistSlug,
            IsPrimary = true,
          },
        },
        CreatedAt = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc(),
      };

      Context.Artists.Add(queen);
      Context.SaveChanges();

      CreateNewArtistRequest request = new CreateNewArtistRequest
      {
        FirstName = firstName,
        LastName = lastName,
      };

      // act
      Func<Task> action = async () => await artistsService.CreateNewArtistAsync(request);

      // assert
      await action.Should().ThrowAsync<ArtistExistsException>();
    }

    [Test]
    public async Task CreateNewArtistAsync_WhenArtistDoesNotAlreadyExists_CreatesNewArtist()
    {
      // arrange
      string firstName = "Katie";
      string lastName = "Melua";
      string fullName = "Katie Melua";
      string artistSlug = "katie-melua";

      artistSlugsServiceMock
        .Setup(x => x.GetArtistSlug(fullName))
        .Returns(artistSlug);

      artistSlugsServiceMock
        .Setup(x => x.BuildArtistSlug(fullName))
        .Returns(new ArtistSlug
        {
          Name = artistSlug,
          CreatedAt = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc(),
        });

      CreateNewArtistRequest request = new CreateNewArtistRequest
      {
        FirstName = firstName,
        LastName = lastName,
      };

      // act
      CreateNewArtistResponse result = await artistsService.CreateNewArtistAsync(request);

      // assert
      result.Should().BeOfType<CreateNewArtistResponse>();
      result.Slug.Should().Be(artistSlug);
      result.CreatedAt.Should().BeCloseTo(SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc(), 100);
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
      PagedArtistSearchResponse result = await artistsService.SearchArtistsAsync(artistName, offset, limit);

      // assert
      result.Should().NotBeNull();
      result.Should().BeOfType<PagedArtistSearchResponse>();
      result.Artists.Should().HaveCount(0);
    }

    [Test]
    public async Task SearchArtistsAsync_WhenThereIsAMatchOnFirstNameOnly_ReturnsAPopulatedListOfArtistCardViewModels()
    {
      // arrange
      string artistName = "fats";
      int offset = 0;
      int limit = 10;

      string artistFirstName = "fats";
      string artistLastName = "waller";
      string artistFullName = $"{artistFirstName} {artistLastName}";
      string artistSlug = "something-different-from-first-and-last-name";
      DateTime artistCreatedAt = DateTime.UtcNow;

      string expectedFullName = textInfo.ToTitleCase(artistFullName);

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
      PagedArtistSearchResponse result = await artistsService.SearchArtistsAsync(artistName, offset, limit);

      // assert
      result.Should().NotBeNull();
      result.Should().BeOfType<PagedArtistSearchResponse>();

      ICollection<ArtistSearchResponse> artists = result.Artists;

      artists.Should().HaveCount(1);

      ArtistSearchResponse artist = artists.First();

      artist.FullName.Should().Be(expectedFullName);
      artist.PrimarySlug.Should().Be(artistSlug);
      artist.HasImage.Should().BeTrue();
    }

    [Test]
    public async Task SearchArtistsAsync_WhenThereIsAMatchOnLastNameOnly_ReturnsAPopulatedListOfArtistCardViewModels()
    {
      // arrange
      string artistName = "waller";
      int offset = 0;
      int limit = 10;

      string artistFirstName = "fats";
      string artistLastName = "waller";
      string artistFullName = $"{artistFirstName} {artistLastName}";
      string artistSlug = "something-different-from-first-and-last-name";
      DateTime artistCreatedAt = DateTime.UtcNow;

      string expectedFullName = textInfo.ToTitleCase(artistFullName);

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
      PagedArtistSearchResponse result = await artistsService.SearchArtistsAsync(artistName, offset, limit);

      // assert
      result.Should().NotBeNull();
      result.Should().BeOfType<PagedArtistSearchResponse>();

      ICollection<ArtistSearchResponse> artists = result.Artists;

      artists.Should().HaveCount(1);

      ArtistSearchResponse artist = artists.First();

      artist.FullName.Should().Be(expectedFullName);
      artist.PrimarySlug.Should().Be(artistSlug);
      artist.HasImage.Should().BeTrue();
    }

    [Test]
    public async Task SearchArtistsAsync_WhenThereIsAMatchOnArtistSlugOnly_ReturnsAPopulatedListOfArtistCardViewModels()
    {
      // arrange
      string artistName = "nokia";
      int offset = 0;
      int limit = 10;

      string artistFirstName = "fats";
      string artistLastName = "waller";
      string artistFullName = $"{artistFirstName} {artistLastName}";
      string artistSlug = "nokia";
      DateTime artistCreatedAt = DateTime.UtcNow;

      string expectedFullName = textInfo.ToTitleCase(artistFullName);

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
      PagedArtistSearchResponse result = await artistsService.SearchArtistsAsync(artistName, offset, limit);

      // assert
      result.Should().NotBeNull();
      result.Should().BeOfType<PagedArtistSearchResponse>();
      ICollection<ArtistSearchResponse> artists = result.Artists;

      artists.Should().HaveCount(1);

      ArtistSearchResponse artist = artists.First();

      artist.FullName.Should().Be(expectedFullName);
      artist.PrimarySlug.Should().Be(artistSlug);
      artist.HasImage.Should().BeTrue();
    }

    [Test]
    public async Task SearchArtistsAsync_WhenSearchParamHasASpaceInItAndThereIsAMatch_ReturnsAPopulatedListOfArtistCardViewModels()
    {
      // arrange
      string artistName = "fats wal";
      int offset = 0;
      int limit = 10;

      string artistFirstName = "fats";
      string artistLastName = "waller";
      string artistFullName = $"{artistFirstName} {artistLastName}";
      string artistSlug = "fats-waller";
      DateTime artistCreatedAt = DateTime.UtcNow;

      string expectedFullName = textInfo.ToTitleCase(artistFullName);

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
      PagedArtistSearchResponse result = await artistsService.SearchArtistsAsync(artistName, offset, limit);

      // assert
      result.Should().NotBeNull();
      result.Should().BeOfType<PagedArtistSearchResponse>();
      ICollection<ArtistSearchResponse> artists = result.Artists;

      artists.Should().HaveCount(1);

      ArtistSearchResponse artist = artists.First();

      artist.FullName.Should().Be(expectedFullName);
      artist.PrimarySlug.Should().Be(artistSlug);
      artist.HasImage.Should().BeTrue();
    }
  }
}
