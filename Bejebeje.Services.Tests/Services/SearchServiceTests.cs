namespace Bejebeje.Services.Tests.Services
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Common.Enums;
  using Bejebeje.Domain;
  using Bejebeje.Services.Services;
  using Bejebeje.Services.Tests.Helpers;
  using Bejebeje.ViewModels.Search;
  using FluentAssertions;
  using Microsoft.Extensions.Logging;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class SearchServiceTests : DatabaseTestBase
  {
    private Mock<ILogger<SearchService>> loggerMock;

    private SearchService searchService;

    [SetUp]
    public void Setup()
    {
      loggerMock = new Mock<ILogger<SearchService>>(MockBehavior.Loose);

      SetupDataContext();

      searchService = new SearchService(
        Context,
        loggerMock.Object);
    }

    [Test]
    public async Task SearchAsync_WhenNoResultsMatch_ReturnsAnEmptyListOfSearchResultViewModels()
    {
      // arrange
      string searchTerm = "queen";

      // act
      IList<SearchResultViewModel> result = await searchService.SearchAsync(searchTerm);

      // assert
      result.Should().NotBeNull();
      result.Should().BeOfType<List<SearchResultViewModel>>();
      result.Should().HaveCount(0);
    }

    [Test]
    public async Task SearchAsync_WhenResultsMatchOnArtistFirstNameOnly_ReturnsAPopulatedListOfSearchResultViewModelsWithCorrectData()
    {
      // arrange
      string baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;
      string searchTerm = "queen";

      string firstArtistName = "Queen";
      string firstArtistSlug = "queen";
      string queenFilePath = baseDirectoryPath + "/Assets/queen.jpg";
      byte[] queenImageBytes = await File.ReadAllBytesAsync(queenFilePath);

      string secondArtistName = "Westlife";
      string secondArtistSlug = "westlife";
      string westlifeFilePath = baseDirectoryPath + "/Assets/westlife.jpg";
      byte[] westlifeImageBytes = await File.ReadAllBytesAsync(westlifeFilePath);

      Artist queen = new Artist
      {
        FirstName = firstArtistName,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = firstArtistSlug,
            IsPrimary = true,
            CreatedAt = DateTime.UtcNow
          }
        },
        Image = new ArtistImage
        {
          Data = queenImageBytes,
          CreatedAt = DateTime.UtcNow
        },
        CreatedAt = DateTime.UtcNow,
        IsApproved = true
      };

      Artist westlife = new Artist
      {
        FirstName = secondArtistName,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = secondArtistSlug,
            IsPrimary = true,
            CreatedAt = DateTime.UtcNow
          }
        },
        Image = new ArtistImage
        {
          Data = westlifeImageBytes,
          CreatedAt = DateTime.UtcNow
        },
        CreatedAt = DateTime.UtcNow,
        IsApproved = true
      };

      Context.Artists.Add(queen);
      Context.Artists.Add(westlife);
      Context.SaveChanges();

      // act
      IList<SearchResultViewModel> result = await searchService.SearchAsync(searchTerm);

      // assert
      result.Should().NotBeNull();
      result.Should().BeOfType<List<SearchResultViewModel>>();
      result.Should().HaveCount(1);
      result.First().Name.Should().Be(firstArtistName);
      result.First().ImageId.Should().Be(1);
      result.First().Slug.Should().Be(firstArtistSlug);
      result.First().ResultType.Should().Be(ResultType.Artist);
    }

    [Test]
    public async Task SearchAsync_WhenResultsMatchOnArtistLastNameOnly_ReturnsAPopulatedListOfSearchResultViewModelsWithCorrectData()
    {
      // arrange
      string baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;
      string searchTerm = "waller";

      string firstArtistName = "Queen";
      string firstArtistSlug = "queen";
      string queenFilePath = baseDirectoryPath + "/Assets/queen.jpg";
      byte[] queenImageBytes = await File.ReadAllBytesAsync(queenFilePath);

      string secondArtistFirstName = "Fats";
      string secondArtistLastName = "Waller";
      string secondArtistSlug = "fats-waller";
      string fatsWallerFilePath = baseDirectoryPath + "/Assets/fats-waller.jpg";
      byte[] fatsWallerImageBytes = await File.ReadAllBytesAsync(fatsWallerFilePath);

      Artist queen = new Artist
      {
        FirstName = firstArtistName,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = firstArtistSlug,
            IsPrimary = true,
            CreatedAt = DateTime.UtcNow
          }
        },
        Image = new ArtistImage
        {
          Data = queenImageBytes,
          CreatedAt = DateTime.UtcNow
        },
        CreatedAt = DateTime.UtcNow,
        IsApproved = true
      };

      Artist fatsWaller = new Artist
      {
        FirstName = secondArtistFirstName,
        LastName = secondArtistLastName,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = secondArtistSlug,
            IsPrimary = true,
            CreatedAt = DateTime.UtcNow
          }
        },
        Image = new ArtistImage
        {
          Data = fatsWallerImageBytes,
          CreatedAt = DateTime.UtcNow
        },
        CreatedAt = DateTime.UtcNow,
        IsApproved = true
      };

      Context.Artists.Add(queen);
      Context.Artists.Add(fatsWaller);
      Context.SaveChanges();

      // act
      IList<SearchResultViewModel> result = await searchService.SearchAsync(searchTerm);

      // assert
      result.Should().NotBeNull();
      result.Should().BeOfType<List<SearchResultViewModel>>();
      result.Should().HaveCount(1);
      result.First().Name.Should().Be($"{secondArtistFirstName} {secondArtistLastName}");
      result.First().ImageId.Should().Be(2);
      result.First().Slug.Should().Be(secondArtistSlug);
      result.First().ResultType.Should().Be(ResultType.Artist);
    }

    [Test]
    public async Task SearchAsync_WhenResultsMatchOnAArtistSlugOnly_ReturnsAPopulatedListOfSearchResultViewModelsWithCorrectData()
    {
      // arrange
      string baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;
      string searchTerm = "unique-slug";

      string firstArtistName = "Queen";
      string firstArtistSlug = "queen";
      string queenFilePath = baseDirectoryPath + "/Assets/queen.jpg";
      byte[] queenImageBytes = await File.ReadAllBytesAsync(queenFilePath);

      string secondArtistName = "Westlife";
      string secondArtistSlug = "westlife";
      string secondArtistUniqueSlug = "unique-slug";
      string lyricTitle = "Queen of My Heart";
      string lyricBody = "Test lyrics";
      string lyricSlug = "queen-of-my-heart";
      string westlifeFilePath = baseDirectoryPath + "/Assets/westlife.jpg";
      byte[] westlifeImageBytes = await File.ReadAllBytesAsync(westlifeFilePath);

      Artist queen = new Artist
      {
        FirstName = firstArtistName,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = firstArtistSlug,
            IsPrimary = true,
            CreatedAt = DateTime.UtcNow
          }
        },
        Image = new ArtistImage
        {
          Data = queenImageBytes,
          CreatedAt = DateTime.UtcNow
        },
        CreatedAt = DateTime.UtcNow,
        IsApproved = true
      };

      Lyric queenOfMyHeart = new Lyric
      {
        Title = lyricTitle,
        Body = lyricBody,
        CreatedAt = DateTime.UtcNow,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = lyricSlug,
            IsPrimary = true,
            CreatedAt = DateTime.UtcNow
          }
        },
        IsApproved = true
      };

      Artist westlife = new Artist
      {
        FirstName = secondArtistName,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = secondArtistSlug,
            IsPrimary = true,
            CreatedAt = DateTime.UtcNow
          },
          new ArtistSlug
          {
            Name = secondArtistUniqueSlug,
            IsPrimary = false,
            CreatedAt = DateTime.UtcNow
          }
        },
        Image = new ArtistImage
        {
          Data = westlifeImageBytes,
          CreatedAt = DateTime.UtcNow
        },
        Lyrics = new List<Lyric> { queenOfMyHeart },
        CreatedAt = DateTime.UtcNow,
        IsApproved = true
      };

      Context.Artists.Add(queen);
      Context.Artists.Add(westlife);
      Context.SaveChanges();

      // act
      IList<SearchResultViewModel> result = await searchService.SearchAsync(searchTerm);

      // assert
      result.Should().NotBeNull();
      result.Should().BeOfType<List<SearchResultViewModel>>();
      result.Should().HaveCount(1);
      result.First().Name.Should().Be(secondArtistName);
      result.First().ImageId.Should().Be(2);
      result.First().Slug.Should().Be(secondArtistSlug);
      result.First().ResultType.Should().Be(ResultType.Artist);
    }
  }
}
