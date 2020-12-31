namespace Bejebeje.Mvc.Tests.Controllers
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.Models.Artist;
  using Bejebeje.Models.Lyric;
  using FluentAssertions;
  using Microsoft.AspNetCore.Mvc;
  using Moq;
  using Mvc.Controllers;
  using NUnit.Framework;
  using Services.Services.Interfaces;

  [TestFixture]
  public class HomeControllerTests
  {
    [Test]
    public async Task Index_ReturnsView()
    {
      // arrange
      IEnumerable<ArtistItemViewModel> tenFemaleArtists = new List<ArtistItemViewModel>
      {
        new ArtistItemViewModel
        {
          FirstName = "A1",
          LastName = "A1",
          ImageAlternateText = "A1",
          ImageUrl = "A1",
          PrimarySlug = "A1",
        },
        new ArtistItemViewModel
        {
          FirstName = "A2",
          LastName = "A2",
          ImageAlternateText = "A2",
          ImageUrl = "A2",
          PrimarySlug = "A2",
        },
        new ArtistItemViewModel
        {
          FirstName = "A3",
          LastName = "A3",
          ImageAlternateText = "A3",
          ImageUrl = "A3",
          PrimarySlug = "A3",
        },
        new ArtistItemViewModel
        {
          FirstName = "A4",
          LastName = "A4",
          ImageAlternateText = "A4",
          ImageUrl = "A4",
          PrimarySlug = "A4",
        },
        new ArtistItemViewModel
        {
          FirstName = "A5",
          LastName = "A5",
          ImageAlternateText = "A5",
          ImageUrl = "A5",
          PrimarySlug = "A5",
        },
        new ArtistItemViewModel
        {
          FirstName = "A6",
          LastName = "A6",
          ImageAlternateText = "A6",
          ImageUrl = "A6",
          PrimarySlug = "A6",
        },
        new ArtistItemViewModel
        {
          FirstName = "A7",
          LastName = "A7",
          ImageAlternateText = "A7",
          ImageUrl = "A7",
          PrimarySlug = "A7",
        },
        new ArtistItemViewModel
        {
          FirstName = "A8",
          LastName = "A8",
          ImageAlternateText = "A8",
          ImageUrl = "A8",
          PrimarySlug = "A8",
        },
        new ArtistItemViewModel
        {
          FirstName = "A9",
          LastName = "A9",
          ImageAlternateText = "A9",
          ImageUrl = "A9",
          PrimarySlug = "A9",
        },
        new ArtistItemViewModel
        {
          FirstName = "A10",
          LastName = "A10",
          ImageAlternateText = "A10",
          ImageUrl = "A10",
          PrimarySlug = "A10",
        }
      };

      Mock<IArtistsService> artistsServiceMock = new Mock<IArtistsService>();

      artistsServiceMock
        .Setup(x => x.GetTopTenFemaleArtistsByLyricsCountAsync())
        .ReturnsAsync(tenFemaleArtists);

      IEnumerable<LyricItemViewModel> tenRecentLyrics = new List<LyricItemViewModel>
      {
        new LyricItemViewModel
        {
          Title = "L1",
          LyricPrimarySlug = "L1",
          ArtistId = 1,
          ArtistName = "L1",
          ArtistPrimarySlug = "L1",
          ArtistImageUrl = "L1",
          ArtistImageAlternateText = "L1",
        },
        new LyricItemViewModel
        {
          Title = "L2",
          LyricPrimarySlug = "L2",
          ArtistId = 2,
          ArtistName = "L2",
          ArtistPrimarySlug = "L2",
          ArtistImageUrl = "L2",
          ArtistImageAlternateText = "L2",
        },
        new LyricItemViewModel
        {
          Title = "L3",
          LyricPrimarySlug = "L3",
          ArtistId = 3,
          ArtistName = "L3",
          ArtistPrimarySlug = "L3",
          ArtistImageUrl = "L3",
          ArtistImageAlternateText = "L3",
        },
        new LyricItemViewModel
        {
          Title = "L4",
          LyricPrimarySlug = "L4",
          ArtistId = 4,
          ArtistName = "L4",
          ArtistPrimarySlug = "L4",
          ArtistImageUrl = "L4",
          ArtistImageAlternateText = "L4",
        },
        new LyricItemViewModel
        {
          Title = "L5",
          LyricPrimarySlug = "L5",
          ArtistId = 5,
          ArtistName = "L5",
          ArtistPrimarySlug = "L5",
          ArtistImageUrl = "L5",
          ArtistImageAlternateText = "L5",
        },
        new LyricItemViewModel
        {
          Title = "L6",
          LyricPrimarySlug = "L6",
          ArtistId = 6,
          ArtistName = "L6",
          ArtistPrimarySlug = "L6",
          ArtistImageUrl = "L6",
          ArtistImageAlternateText = "L6",
        },
        new LyricItemViewModel
        {
          Title = "L7",
          LyricPrimarySlug = "L7",
          ArtistId = 7,
          ArtistName = "L7",
          ArtistPrimarySlug = "L7",
          ArtistImageUrl = "L7",
          ArtistImageAlternateText = "L7",
        },
        new LyricItemViewModel
        {
          Title = "L8",
          LyricPrimarySlug = "L8",
          ArtistId = 8,
          ArtistName = "L8",
          ArtistPrimarySlug = "L8",
          ArtistImageUrl = "L8",
          ArtistImageAlternateText = "L8",
        },
        new LyricItemViewModel
        {
          Title = "L9",
          LyricPrimarySlug = "L9",
          ArtistId = 9,
          ArtistName = "L9",
          ArtistPrimarySlug = "L9",
          ArtistImageUrl = "L9",
          ArtistImageAlternateText = "L9",
        },
        new LyricItemViewModel
        {
          Title = "L10",
          LyricPrimarySlug = "L10",
          ArtistId = 10,
          ArtistName = "L10",
          ArtistPrimarySlug = "L10",
          ArtistImageUrl = "L10",
          ArtistImageAlternateText = "L10",
        },
      };

      Mock<ILyricsService> lyricsServiceMock = new Mock<ILyricsService>();

      lyricsServiceMock.Setup(x => x.GetRecentLyricsAsync()).ReturnsAsync(tenRecentLyrics);

      HomeController homeController = new HomeController(artistsServiceMock.Object, lyricsServiceMock.Object);

      // act
      IActionResult actionResult = await homeController.Index();

      // assert
      actionResult.Should().NotBeNull();
    }
  }
}
