using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bejebeje.Common.Exceptions;
using Bejebeje.Domain;
using Bejebeje.Services.Services;
using Bejebeje.Services.Tests.Helpers;
using Bejebeje.ViewModels.Lyric;
using FluentAssertions;
using NUnit.Framework;

namespace Bejebeje.Services.Tests.Services
{
  [TestFixture]
  public class LyricServiceTests : DatabaseTestBase
  {
    private LyricsService lyricsService;

    public List ListCardViewModel { get; private set; }

    [SetUp]
    public void Setup()
    {
      SetupDataContext();

      lyricsService = new LyricsService(Context);
    }

    [Test]
    public async Task GetLyricsAsync_WhenArtistDoesNotExist_ThrowsAnArtistNotFoundException()
    {
      // arrange
      string artistSlug = "john-doe";

      // act
      Func<Task> act = async () => await lyricsService.GetLyricsAsync(artistSlug);

      // assert
      await act.Should().ThrowAsync<ArtistNotFoundException>();
    }

    [Test]
    public async Task GetLyricsAsync_WhenArtistDoesExistButHasNoLyrics_ReturnsAnEmptyListOfLyrics()
    {
      // arrange
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
      
      // act
      IList<LyricCardViewModel> result = await lyricsService.GetLyricsAsync(artistSlug);

      // assert
      result.Should().NotBeEmpty();
      result.First().Title.Should().Be(lyricTitle);
      result.First().Slug.Should().Be(lyricSlug);
    }
  }
}
