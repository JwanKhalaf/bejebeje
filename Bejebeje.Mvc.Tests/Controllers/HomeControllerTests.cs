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
    public async Task Index_ReturnsAViewResult_WithAnIndexViewModel()
    {
      // arrange
      IEnumerable<RandomFemaleArtistItemViewModel> tenFemaleArtists = GetListOfFemaleArtists();

      Mock<IArtistsService> mockArtistsService = new Mock<IArtistsService>();

      mockArtistsService
        .Setup(x => x.GetTopTenFemaleArtistsByLyricsCountAsync())
        .ReturnsAsync(tenFemaleArtists);

      IEnumerable<LyricItemViewModel> tenRecentLyrics = GetListOfRecentLyrics();

      Mock<ILyricsService> mockLyricsService = new Mock<ILyricsService>();

      mockLyricsService
        .Setup(x => x.GetRecentlySubmittedLyricsAsync())
        .ReturnsAsync(tenRecentLyrics);

      HomeController homeController = new HomeController(mockArtistsService.Object, mockLyricsService.Object);

      // act
      IActionResult actionResult = await homeController.Index();

      // assert
      ViewResult view = actionResult.Should().BeOfType<ViewResult>().Subject;
      IndexViewModel viewModel = view.Model.Should().BeOfType<IndexViewModel>().Subject;
      viewModel.FemaleArtists.Should().HaveCount(10);
      viewModel.RecentlySubmittedLyrics.Should().HaveCount(10);
    }

    private static IEnumerable<LyricItemViewModel> GetListOfRecentLyrics()
    {
      return new List<LyricItemViewModel>
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
    }

    private static IEnumerable<RandomFemaleArtistItemViewModel> GetListOfFemaleArtists()
    {
      return new List<RandomFemaleArtistItemViewModel>
      {
        new RandomFemaleArtistItemViewModel
        {
          Name = "A1",
          ImageAlternateText = "A1",
          ImageUrl = "A1",
          PrimarySlug = "A1",
        },
        new RandomFemaleArtistItemViewModel
        {
          Name = "A2",
          ImageAlternateText = "A2",
          ImageUrl = "A2",
          PrimarySlug = "A2",
        },
        new RandomFemaleArtistItemViewModel
        {
          Name = "A3",
          ImageAlternateText = "A3",
          ImageUrl = "A3",
          PrimarySlug = "A3",
        },
        new RandomFemaleArtistItemViewModel
        {
          Name = "A4",
          ImageAlternateText = "A4",
          ImageUrl = "A4",
          PrimarySlug = "A4",
        },
        new RandomFemaleArtistItemViewModel
        {
          Name = "A5",
          ImageAlternateText = "A5",
          ImageUrl = "A5",
          PrimarySlug = "A5",
        },
        new RandomFemaleArtistItemViewModel
        {
          Name = "A6",
          ImageAlternateText = "A6",
          ImageUrl = "A6",
          PrimarySlug = "A6",
        },
        new RandomFemaleArtistItemViewModel
        {
          Name = "A7",
          ImageAlternateText = "A7",
          ImageUrl = "A7",
          PrimarySlug = "A7",
        },
        new RandomFemaleArtistItemViewModel
        {
          Name = "A8",
          ImageAlternateText = "A8",
          ImageUrl = "A8",
          PrimarySlug = "A8",
        },
        new RandomFemaleArtistItemViewModel
        {
          Name = "A9",
          ImageAlternateText = "A9",
          ImageUrl = "A9",
          PrimarySlug = "A9",
        },
        new RandomFemaleArtistItemViewModel
        {
          Name = "A10",
          ImageAlternateText = "A10",
          ImageUrl = "A10",
          PrimarySlug = "A10",
        }
      };
    }
  }
}