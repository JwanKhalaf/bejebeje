using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bejebeje.Common.Exceptions;
using Bejebeje.Domain;
using Bejebeje.Services.Services;
using Bejebeje.Services.Services.Interfaces;
using Bejebeje.Services.Tests.Helpers;
using Bejebeje.ViewModels.Lyric;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Bejebeje.Services.Tests.Services
{
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
        .ReturnsAsync(0);

      // act
      Func<Task> act = async () => await lyricsService.GetLyricsAsync(artistSlug);

      // assert
      await act.Should().ThrowAsync<ArtistNotFoundException>();
    }

    [Test]
    public async Task GetLyricsAsync_WhenArtistDoesExistButHasNoLyrics_ReturnsAnEmptyListOfLyrics()
    {
      // arrange
      int artistId = 1;
      string artistSlug = "fats-waller";
      string artistFirstName = "Fats";
      string artistLastName = "Waller";

      string lyricSlug = "write-myself-a-letter";
      string lyricTitle = "Write Myself A Letter";
      string lyricBody = "Song lyrics";

      Lyric writeMyselfALetterSong = new Lyric
      {
        Title = lyricTitle,
        Body = lyricBody,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = lyricSlug,
            CreatedAt = DateTime.UtcNow,
            IsPrimary = true
          }
        }
      };

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
        Lyrics = new List<Lyric> { writeMyselfALetterSong }
      };

      Context.Artists.Add(fatsWaller);
      Context.SaveChanges();

      artistsServiceMock
        .Setup(x => x.GetArtistIdAsync(artistSlug))
        .ReturnsAsync(artistId);

      // act
      IList<LyricCardViewModel> result = await lyricsService.GetLyricsAsync(artistSlug);

      // assert
      result.Should().NotBeEmpty();
      result.First().Title.Should().Be(lyricTitle);
      result.First().Slug.Should().Be(lyricSlug);
    }

    [Test]
    public async Task GetSingleLyricAsync_WhenArtistDoesNotExist_ThrowsAnArtistNotFoundException()
    {
      // arrange
      string artistSlug = "john-doe";
      string lyricSlug = "test-song";

      artistsServiceMock
        .Setup(x => x.GetArtistIdAsync(artistSlug))
        .ReturnsAsync(0);

      // act
      Func<Task> act = async () => await lyricsService.GetSingleLyricAsync(artistSlug, lyricSlug);

      // assert
      await act.Should().ThrowAsync<ArtistNotFoundException>();
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
            IsPrimary = true
          }
        }
      };

      Context.Artists.Add(adaBrown);
      Context.SaveChanges();

      artistsServiceMock
        .Setup(x => x.GetArtistIdAsync(artistSlug))
        .ReturnsAsync(artistId);

      // act
      Func<Task> act = async () => await lyricsService.GetSingleLyricAsync(artistSlug, lyricSlug);

      // assert
      await act.Should().ThrowAsync<LyricNotFoundException>();
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

      Lyric evilMamaBlue = new Lyric
      {
        Title = lyricTitle,
        Body = lyricBody,
        CreatedAt = DateTime.UtcNow,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = lyricSlug,
            CreatedAt = DateTime.UtcNow,
            IsPrimary = true
          }
        }
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
            CreatedAt = DateTime.UtcNow,
            IsPrimary = true
          }
        },
        Lyrics = new List<Lyric> { evilMamaBlue }
      };

      Context.Artists.Add(adaBrown);
      Context.SaveChanges();

      artistsServiceMock
        .Setup(x => x.GetArtistIdAsync(artistSlug))
        .ReturnsAsync(artistId);

      // act
      LyricViewModel result =  await lyricsService.GetSingleLyricAsync(artistSlug, lyricSlug);

      // assert
      result.Should().NotBeNull();
      result.Title.Should().Be(lyricTitle);
      result.Body.Should().Be(lyricBody);
    }
  }
}
