namespace Bejebeje.Services.Tests.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Domain;
  using Bejebeje.Models.Lyric;
  using Bejebeje.Services.Services;
  using Bejebeje.Services.Services.Interfaces;
  using Bejebeje.Services.Tests.Helpers;
  using FluentAssertions;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class LyricServiceTests : DatabaseTestBase
  {
    private Mock<IArtistsService> artistsServiceMock;

    private LyricsService lyricsService;

    public List ListCardViewModel { get; private set; }

    [SetUp]
    public void Setup()
    {
      SetupDataContext();

      artistsServiceMock = new Mock<IArtistsService>(MockBehavior.Strict);

      lyricsService = new LyricsService(artistsServiceMock.Object, Context);
    }

    [Test]
    public async Task GetLyricsAsync_WhenArtistDoesNotExist_ThrowsAnArtistNotFoundException()
    {
      // arrange
      string artistSlug = "john-doe";

      artistsServiceMock
        .Setup(x => x.GetArtistIdAsync(artistSlug))
        .ThrowsAsync(new ArtistNotFoundException(artistSlug));

      // act
      Func<Task> action = async () => await lyricsService.GetLyricsAsync(artistSlug);

      // assert
      await action.Should().ThrowAsync<ArtistNotFoundException>();
    }

    [Test]
    public async Task GetLyricsAsync_WhenArtistDoesExistButHasNoLyrics_ReturnsAnEmptyListOfLyrics()
    {
      // arrange
      int artistId = 1;
      string artistSlug = "fats-waller";
      string artistFirstName = "Fats";
      string artistLastName = "Waller";
      bool isDeleted = false;
      bool isApproved = true;

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
            IsPrimary = true,
          },
        },
        IsDeleted = isDeleted,
        IsApproved = isApproved,
      };

      Context.Artists.Add(fatsWaller);
      Context.SaveChanges();

      artistsServiceMock
        .Setup(x => x.GetArtistIdAsync(artistSlug))
        .ReturnsAsync(artistId);

      // act
      IList<LyricCardViewModel> result = await lyricsService.GetLyricsAsync(artistSlug);

      // assert
      result.Should().BeOfType<List<LyricCardViewModel>>();
      result.Should().BeEmpty();
    }

    [Test]
    public async Task SearchLyricsAsync_WhenNoLyricsMatch_ReturnsAPagedLyricSearchResponseWithEmptyListOfLyricSearchResponse()
    {
      // arrange
      int offset = 0;
      int limit = 10;
      string lyricTitle = "window";
      string seedLyricTitle = "TNT";
      string seedLyricSlug = "tnt";
      DateTime seedLyricCreatedAt = DateTime.UtcNow;

      Lyric tnt = new Lyric
      {
        Title = seedLyricTitle,
        CreatedAt = seedLyricCreatedAt,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = seedLyricSlug,
            IsPrimary = true,
            CreatedAt = seedLyricCreatedAt,
          },
        },
      };

      Context.Lyrics.Add(tnt);
      Context.SaveChanges();

      // act
      PagedLyricSearchResponse result = await lyricsService
        .SearchLyricsAsync(lyricTitle, offset, limit);

      // assert
      result.Should().BeOfType<PagedLyricSearchResponse>();
      result.Lyrics.Should().BeEmpty();
    }

    [Test]
    public async Task SearchLyricsAsync_WhenThereIsMatchButAllLyricsAreDeleted_ReturnsAPagedLyricSearchResponseWithEmptyListOfLyricSearchResponse()
    {
      // arrange
      int offset = 0;
      int limit = 10;

      string artistFirstName = "ac/dc";
      string artistSlug = "acdc";
      DateTime artistCreatedAt = DateTime.UtcNow;
      bool artistIsDeleted = false;
      bool artistIsApproved = true;

      string lyricTitle = "tnt";
      string seedLyricTitle = "TNT";
      string unmatchedLyricSlug = "door";
      DateTime seedLyricCreatedAt = DateTime.UtcNow;
      bool lyricIsDeleted = true;
      bool lyricIsApproved = true;

      Lyric tnt = new Lyric
      {
        Title = seedLyricTitle,
        CreatedAt = seedLyricCreatedAt,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = unmatchedLyricSlug,
            IsPrimary = true,
            CreatedAt = seedLyricCreatedAt,
          },
        },
        Artist = new Artist
        {
          FirstName = artistFirstName,
          FullName = artistFirstName,
          CreatedAt = artistCreatedAt,
          Slugs = new List<ArtistSlug>
          {
            new ArtistSlug
            {
              Name = artistSlug,
              CreatedAt = artistCreatedAt,
              IsPrimary = true,
            },
          },
          IsDeleted = artistIsDeleted,
          IsApproved = artistIsApproved,
        },
        IsDeleted = lyricIsDeleted,
        IsApproved = lyricIsApproved,
      };

      Context.Lyrics.Add(tnt);
      Context.SaveChanges();

      // act
      PagedLyricSearchResponse result = await lyricsService
        .SearchLyricsAsync(lyricTitle, offset, limit);

      // assert
      result.Should().BeOfType<PagedLyricSearchResponse>();
      result.Lyrics.Should().BeEmpty();
    }

    [Test]
    public async Task SearchLyricsAsync_WhenThereIsMatchButOnlySomeLyricsAreDeleted_ReturnsAPagedLyricSearchResponseWithPopulatedListOfLyricSearchResponse()
    {
      // arrange
      int offset = 0;
      int limit = 10;

      string lyricTitleToSearch = "thunder";

      string artistFirstName = "ac/dc";
      string artistSlug = "acdc";
      DateTime artistCreatedAt = DateTime.UtcNow;
      bool artistIsDeleted = false;
      bool artistIsApproved = true;

      Artist acdc = new Artist
      {
        FirstName = artistFirstName,
        FullName = artistFirstName,
        CreatedAt = artistCreatedAt,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = artistSlug,
            CreatedAt = artistCreatedAt,
            IsPrimary = true,
          },
        },
        IsDeleted = artistIsDeleted,
        IsApproved = artistIsApproved,
      };

      Context.Artists.Add(acdc);
      Context.SaveChanges();

      string seedLyricTitleOne = "TNT";
      string seedLyricSlugOne = "tnt";
      DateTime seedLyricCreatedAtOne = DateTime.UtcNow;
      bool lyricIsDeletedOne = true;
      bool lyricIsApprovedOne = true;

      string seedLyricTitleTwo = "Thunderstruck";
      string seedLyricSlugTwo = "thunderstruck";
      DateTime seedLyricCreatedAtTwo = DateTime.UtcNow;
      bool lyricIsDeletedTwo = false;
      bool lyricIsApprovedTwo = true;

      Lyric tnt = new Lyric
      {
        Title = seedLyricTitleOne,
        CreatedAt = seedLyricCreatedAtOne,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = seedLyricSlugOne,
            IsPrimary = true,
            CreatedAt = seedLyricCreatedAtOne,
          },
        },
        Artist = acdc,
        IsDeleted = lyricIsDeletedOne,
        IsApproved = lyricIsApprovedOne,
      };

      Lyric thunderstruck = new Lyric
      {
        Title = seedLyricTitleTwo,
        CreatedAt = seedLyricCreatedAtTwo,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = seedLyricSlugTwo,
            IsPrimary = true,
            CreatedAt = seedLyricCreatedAtTwo,
          },
        },
        Artist = acdc,
        IsDeleted = lyricIsDeletedTwo,
        IsApproved = lyricIsApprovedTwo,
      };

      Context.Lyrics.Add(tnt);
      Context.Lyrics.Add(thunderstruck);
      Context.SaveChanges();

      // act
      PagedLyricSearchResponse result = await lyricsService
        .SearchLyricsAsync(lyricTitleToSearch, offset, limit);

      // assert
      result.Should().BeOfType<PagedLyricSearchResponse>();
      result.Lyrics.Should().NotBeEmpty();
      result.Lyrics.Should().HaveCount(1);
      result.Lyrics.First().Title.Should().Be(seedLyricTitleTwo);
      result.Lyrics.First().PrimarySlug.Should().Be(seedLyricSlugTwo);
    }

    [Test]
    public async Task SearchLyricsAsync_WhenThereIsMatchButAllLyricsAreUnapproved_ReturnsAPagedLyricSearchResponseWithEmptyListOfLyricSearchResponse()
    {
      // arrange
      int offset = 0;
      int limit = 10;

      string lyricTitleToSearch = "tnt";

      string artistFirstName = "ac/dc";
      string artistSlug = "acdc";
      DateTime artistCreatedAt = DateTime.UtcNow;
      bool artistIsDeleted = false;
      bool artistIsApproved = true;

      string seedLyricTitle = "TNT";
      string unmatchedLyricSlug = "door";
      DateTime seedLyricCreatedAt = DateTime.UtcNow;
      bool lyricIsDeleted = false;
      bool lyricIsApproved = false;

      Lyric tnt = new Lyric
      {
        Title = seedLyricTitle,
        CreatedAt = seedLyricCreatedAt,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = unmatchedLyricSlug,
            IsPrimary = true,
            CreatedAt = seedLyricCreatedAt,
          },
        },
        Artist = new Artist
        {
          FirstName = artistFirstName,
          FullName = artistFirstName,
          CreatedAt = artistCreatedAt,
          Slugs = new List<ArtistSlug>
          {
            new ArtistSlug
            {
              Name = artistSlug,
              CreatedAt = artistCreatedAt,
              IsPrimary = true,
            },
          },
          IsDeleted = artistIsDeleted,
          IsApproved = artistIsApproved,
        },
        IsDeleted = lyricIsDeleted,
        IsApproved = lyricIsApproved,
      };

      Context.Lyrics.Add(tnt);
      Context.SaveChanges();

      // act
      PagedLyricSearchResponse result = await lyricsService
        .SearchLyricsAsync(lyricTitleToSearch, offset, limit);

      // assert
      result.Should().BeOfType<PagedLyricSearchResponse>();
      result.Lyrics.Should().BeEmpty();
    }

    [Test]
    public async Task SearchLyricsAsync_WhenThereIsMatchButOnlySomeLyricsAreApproved_ReturnsAPagedLyricSearchResponseWithPopulatedListOfLyricSearchResponse()
    {
      // arrange
      int offset = 0;
      int limit = 10;

      string lyricTitleToSearch = "tnt";

      string artistFirstName = "ac/dc";
      string artistSlug = "acdc";
      DateTime artistCreatedAt = DateTime.UtcNow;
      bool artistIsDeleted = false;
      bool artistIsApproved = true;

      Artist acdc = new Artist
      {
        FirstName = artistFirstName,
        FullName = artistFirstName,
        CreatedAt = artistCreatedAt,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = artistSlug,
            CreatedAt = artistCreatedAt,
            IsPrimary = true,
          },
        },
        IsDeleted = artistIsDeleted,
        IsApproved = artistIsApproved,
      };

      Context.Artists.Add(acdc);
      Context.SaveChanges();

      string seedLyricTitleOne = "TNT";
      string seedLyricSlugOne = "tnt";
      DateTime seedLyricCreatedAtOne = DateTime.UtcNow;
      bool lyricIsDeletedOne = false;
      bool lyricIsApprovedOne = true;

      string seedLyricTitleTwo = "Thunderstruck";
      string seedLyricSlugTwo = "thunderstruck";
      DateTime seedLyricCreatedAtTwo = DateTime.UtcNow;
      bool lyricIsDeletedTwo = false;
      bool lyricIsApprovedTwo = false;

      Lyric tnt = new Lyric
      {
        Title = seedLyricTitleOne,
        CreatedAt = seedLyricCreatedAtOne,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = seedLyricSlugOne,
            IsPrimary = true,
            CreatedAt = seedLyricCreatedAtOne,
          },
        },
        Artist = acdc,
        IsDeleted = lyricIsDeletedOne,
        IsApproved = lyricIsApprovedOne,
      };

      Lyric thunderstruck = new Lyric
      {
        Title = seedLyricTitleTwo,
        CreatedAt = seedLyricCreatedAtTwo,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = seedLyricSlugTwo,
            IsPrimary = true,
            CreatedAt = seedLyricCreatedAtTwo,
          },
        },
        Artist = acdc,
        IsDeleted = lyricIsDeletedTwo,
        IsApproved = lyricIsApprovedTwo,
      };

      Context.Lyrics.Add(tnt);
      Context.Lyrics.Add(thunderstruck);
      Context.SaveChanges();

      // act
      PagedLyricSearchResponse result = await lyricsService
        .SearchLyricsAsync(lyricTitleToSearch, offset, limit);

      // assert
      result.Should().BeOfType<PagedLyricSearchResponse>();
      result.Lyrics.Should().NotBeEmpty();
      result.Lyrics.Should().HaveCount(1);
      result.Lyrics.First().Title.Should().Be(seedLyricTitleOne);
      result.Lyrics.First().PrimarySlug.Should().Be(seedLyricSlugOne);
    }

    [Test]
    public async Task SearchLyricsAsync_WhenThereIsMatchButOnlySomeLyricsAreApprovedAndNotDeleted_ReturnsAPagedLyricSearchResponseWithPopulatedListOfLyricSearchResponse()
    {
      // arrange
      int offset = 0;
      int limit = 10;

      string lyricTitleToSearch = "back in";

      string artistFirstName = "ac/dc";
      string artistSlug = "acdc";
      DateTime artistCreatedAt = DateTime.UtcNow;
      bool artistIsDeleted = false;
      bool artistIsApproved = true;

      Artist acdc = new Artist
      {
        FirstName = artistFirstName,
        FullName = artistFirstName,
        CreatedAt = artistCreatedAt,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = artistSlug,
            CreatedAt = artistCreatedAt,
            IsPrimary = true,
          },
        },
        IsDeleted = artistIsDeleted,
        IsApproved = artistIsApproved,
      };

      Context.Artists.Add(acdc);
      Context.SaveChanges();

      string seedLyricTitleOne = "TNT";
      string seedLyricSlugOne = "tnt";
      DateTime seedLyricCreatedAtOne = DateTime.UtcNow;
      bool lyricIsDeletedOne = true;
      bool lyricIsApprovedOne = true;

      string seedLyricTitleTwo = "Thunderstruck";
      string seedLyricSlugTwo = "thunderstruck";
      DateTime seedLyricCreatedAtTwo = DateTime.UtcNow;
      bool lyricIsDeletedTwo = false;
      bool lyricIsApprovedTwo = false;

      string seedLyricTitleThree = "Back in black";
      string seedLyricSlugThree = "back-in-black";
      DateTime seedLyricCreatedAtThree = DateTime.UtcNow;
      bool lyricIsDeletedThree = false;
      bool lyricIsApprovedThree = true;

      Lyric tnt = new Lyric
      {
        Title = seedLyricTitleOne,
        CreatedAt = seedLyricCreatedAtOne,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = seedLyricSlugOne,
            IsPrimary = true,
            CreatedAt = seedLyricCreatedAtOne,
          },
        },
        Artist = acdc,
        IsDeleted = lyricIsDeletedOne,
        IsApproved = lyricIsApprovedOne,
      };

      Lyric thunderstruck = new Lyric
      {
        Title = seedLyricTitleTwo,
        CreatedAt = seedLyricCreatedAtTwo,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = seedLyricSlugTwo,
            IsPrimary = true,
            CreatedAt = seedLyricCreatedAtTwo,
          },
        },
        Artist = acdc,
        IsDeleted = lyricIsDeletedTwo,
        IsApproved = lyricIsApprovedTwo,
      };

      Lyric backInBlack = new Lyric
      {
        Title = seedLyricTitleThree,
        CreatedAt = seedLyricCreatedAtThree,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = seedLyricSlugThree,
            IsPrimary = true,
            CreatedAt = seedLyricCreatedAtThree,
          },
        },
        Artist = acdc,
        IsDeleted = lyricIsDeletedThree,
        IsApproved = lyricIsApprovedThree,
      };

      Context.Lyrics.Add(tnt);
      Context.Lyrics.Add(thunderstruck);
      Context.Lyrics.Add(backInBlack);
      Context.SaveChanges();

      // act
      PagedLyricSearchResponse result = await lyricsService
        .SearchLyricsAsync(lyricTitleToSearch, offset, limit);

      // assert
      result.Should().BeOfType<PagedLyricSearchResponse>();
      result.Lyrics.Should().NotBeEmpty();
      result.Lyrics.Should().HaveCount(1);
      result.Lyrics.First().Title.Should().Be(seedLyricTitleThree);
      result.Lyrics.First().PrimarySlug.Should().Be(seedLyricSlugThree);
    }

    [Test]
    public async Task SearchLyricsAsync_WhenThereIsMatchOnTitleOnly_ReturnsAPagedLyricSearchResponseWithPopulatedListOfLyricSearchResponse()
    {
      // arrange
      int offset = 0;
      int limit = 10;

      string lyricTitleToSearch = "tnt";

      string artistFirstName = "ac/dc";
      string artistSlug = "acdc";
      DateTime artistCreatedAt = DateTime.UtcNow;

      string seedLyricTitle = "TNT";
      string unmatchedLyricSlug = "door";
      DateTime seedLyricCreatedAt = DateTime.UtcNow;
      bool isDeleted = false;
      bool isApproved = true;

      Lyric tnt = new Lyric
      {
        Title = seedLyricTitle,
        CreatedAt = seedLyricCreatedAt,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = unmatchedLyricSlug,
            IsPrimary = true,
            CreatedAt = seedLyricCreatedAt,
          },
        },
        Artist = new Artist
        {
          FirstName = artistFirstName,
          FullName = artistFirstName,
          IsApproved = true,
          CreatedAt = artistCreatedAt,
          Slugs = new List<ArtistSlug>
          {
            new ArtistSlug
            {
              Name = artistSlug,
              CreatedAt = artistCreatedAt,
              IsPrimary = true,
            },
          },
        },
        IsDeleted = isDeleted,
        IsApproved = isApproved,
      };

      Context.Lyrics.Add(tnt);
      Context.SaveChanges();

      // act
      PagedLyricSearchResponse result = await lyricsService
        .SearchLyricsAsync(lyricTitleToSearch, offset, limit);

      // assert
      result.Should().BeOfType<PagedLyricSearchResponse>();
      result.Lyrics.Should().NotBeEmpty();
      result.Lyrics.Should().HaveCount(1);
      result.Lyrics.First().Title.Should().Be(seedLyricTitle);
      result.Lyrics.First().PrimarySlug.Should().Be(unmatchedLyricSlug);
    }

    [Test]
    public async Task SearchLyricsAsync_WhenThereIsMatchOnLyricSlugOnly_ReturnsAPagedLyricSearchResponseWithPopulatedListOfLyricSearchResponse()
    {
      // arrange
      int offset = 0;
      int limit = 10;

      string lyricTitleToSearch = "oen";

      string artistFirstName = "ac/dc";
      string artistSlug = "acdc";
      DateTime artistCreatedAt = DateTime.UtcNow;

      string seedLyricTitle = "TNT";
      string uniqueLyricSlug = "uioenkl";
      DateTime seedLyricCreatedAt = DateTime.UtcNow;
      bool isDeleted = false;
      bool isApproved = true;

      Lyric tnt = new Lyric
      {
        Title = seedLyricTitle,
        CreatedAt = seedLyricCreatedAt,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = uniqueLyricSlug,
            IsPrimary = true,
            CreatedAt = seedLyricCreatedAt,
          },
        },
        Artist = new Artist
        {
          FirstName = artistFirstName,
          FullName = artistFirstName,
          IsApproved = true,
          CreatedAt = artistCreatedAt,
          Slugs = new List<ArtistSlug>
          {
            new ArtistSlug
            {
              Name = artistSlug,
              CreatedAt = artistCreatedAt,
              IsPrimary = true,
            },
          },
        },
        IsDeleted = isDeleted,
        IsApproved = isApproved,
      };

      Context.Lyrics.Add(tnt);
      Context.SaveChanges();

      // act
      PagedLyricSearchResponse result = await lyricsService
        .SearchLyricsAsync(lyricTitleToSearch, offset, limit);

      // assert
      result.Should().BeOfType<PagedLyricSearchResponse>();
      result.Lyrics.Should().NotBeEmpty();
      result.Lyrics.Should().HaveCount(1);
      result.Lyrics.First().Title.Should().Be(seedLyricTitle);
      result.Lyrics.First().PrimarySlug.Should().Be(uniqueLyricSlug);
    }

    [Test]
    public async Task SearchLyricsAsync_WhenTheParamHasASpaceAndThereIsMatchOnLyricSlugOnly_ReturnsAPagedLyricSearchResponseWithPopulatedListOfLyricSearchResponse()
    {
      // arrange
      int offset = 0;
      int limit = 10;

      string lyricTitleToSearch = "back in";

      string artistFirstName = "ac/dc";
      string artistSlug = "acdc";
      DateTime artistCreatedAt = DateTime.UtcNow;

      string seedLyricTitle = "Back in black";
      string uniqueLyricSlug = "back-in-black";
      DateTime seedLyricCreatedAt = DateTime.UtcNow;
      bool isDeleted = false;
      bool isApproved = true;

      Lyric tnt = new Lyric
      {
        Title = seedLyricTitle,
        CreatedAt = seedLyricCreatedAt,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = uniqueLyricSlug,
            IsPrimary = true,
            CreatedAt = seedLyricCreatedAt,
          },
        },
        Artist = new Artist
        {
          FirstName = artistFirstName,
          FullName = artistFirstName,
          IsApproved = true,
          CreatedAt = artistCreatedAt,
          Slugs = new List<ArtistSlug>
          {
            new ArtistSlug
            {
              Name = artistSlug,
              CreatedAt = artistCreatedAt,
              IsPrimary = true,
            },
          },
        },
        IsDeleted = isDeleted,
        IsApproved = isApproved,
      };

      Context.Lyrics.Add(tnt);
      Context.SaveChanges();

      // act
      PagedLyricSearchResponse result = await lyricsService
        .SearchLyricsAsync(lyricTitleToSearch, offset, limit);

      // assert
      result.Should().BeOfType<PagedLyricSearchResponse>();
      result.Lyrics.Should().NotBeEmpty();
      result.Lyrics.Should().HaveCount(1);
      result.Lyrics.First().Title.Should().Be(seedLyricTitle);
      result.Lyrics.First().PrimarySlug.Should().Be(uniqueLyricSlug);
    }

    [Test]
    public async Task GetSingleLyricAsync_WhenArtistDoesNotExist_ThrowsAnArtistNotFoundException()
    {
      // arrange
      string artistSlug = "john-doe";
      string lyricSlug = "test-song";

      artistsServiceMock
        .Setup(x => x.GetArtistIdAsync(artistSlug))
        .ThrowsAsync(new ArtistNotFoundException(artistSlug));

      // act
      Func<Task> action = async () => await lyricsService.GetSingleLyricAsync(artistSlug, lyricSlug);

      // assert
      await action.Should().ThrowAsync<ArtistNotFoundException>();
    }

    [Test]
    public async Task GetSingleLyricAsync_WhenTheLyricDoesNotExist_ThrowsALyricNotFoundException()
    {
      // arrange
      int artistId = 1;
      string artistSlug = "ada-brown";
      string lyricSlug = "test-song";

      string artistFirstName = "Ada";
      string artistLastName = "Brown";

      Artist adaBrown = new Artist
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
            IsPrimary = true,
          },
        },
      };

      Context.Artists.Add(adaBrown);
      Context.SaveChanges();

      artistsServiceMock
        .Setup(x => x.GetArtistIdAsync(artistSlug))
        .ReturnsAsync(artistId);

      // act
      Func<Task> action = async () => await lyricsService.GetSingleLyricAsync(artistSlug, lyricSlug);

      // assert
      await action.Should().ThrowAsync<LyricNotFoundException>();
    }

    [Test]
    public async Task GetSingleLyricAsync_WhenBothArtistAndLyricExist_ReturnsTheLyric()
    {
      // arrange
      int artistId = 1;
      string artistSlug = "ada-brown";
      string lyricSlug = "test-song";

      string artistFirstName = "Ada";
      string artistLastName = "Brown";

      string lyricTitle = "Evil Mama Blues";
      string lyricBody = "song lyrics";

      DateTime createdAt = new DateTime(2019, 7, 6, 12, 0, 0, DateTimeKind.Utc);

      Author adaBrownAuthor = new Author
      {
        FirstName = artistFirstName,
        LastName = artistLastName,
        Slugs = new List<AuthorSlug>
        {
          new AuthorSlug
          {
            Name = artistSlug,
            IsPrimary = true,
            CreatedAt = createdAt,
          },
        },
        CreatedAt = createdAt,
      };

      Lyric evilMamaBlue = new Lyric
      {
        Title = lyricTitle,
        Body = lyricBody,
        CreatedAt = createdAt,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = lyricSlug,
            CreatedAt = createdAt,
            IsPrimary = true,
          },
        },
        Author = adaBrownAuthor,
      };

      Artist adaBrown = new Artist
      {
        Id = artistId,
        FirstName = artistFirstName,
        LastName = artistLastName,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = artistSlug,
            CreatedAt = createdAt,
            IsPrimary = true,
          },
        },
        Lyrics = new List<Lyric> { evilMamaBlue },
      };

      Context.Artists.Add(adaBrown);
      Context.SaveChanges();

      artistsServiceMock
        .Setup(x => x.GetArtistIdAsync(artistSlug))
        .ReturnsAsync(artistId);

      // act
      LyricResponse result = await lyricsService.GetSingleLyricAsync(artistSlug, lyricSlug);

      // assert
      result.Should().NotBeNull();
      result.Title.Should().Be(lyricTitle);
      result.Body.Should().Be(lyricBody);
      result.AuthorSlug.Should().Be(artistSlug);
      result.CreatedAt.Should().Be(createdAt);
      result.ModifiedAt.Should().BeNull();
    }
  }
}
