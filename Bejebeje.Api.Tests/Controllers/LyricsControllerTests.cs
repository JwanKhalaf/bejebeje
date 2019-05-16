using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bejebeje.Api.Controllers;
using Bejebeje.Common.Exceptions;
using Bejebeje.Services.Services.Interfaces;
using Bejebeje.ViewModels.Lyric;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Bejebeje.Api.Tests.Controllers
{
  [TestFixture]
  public class LyricsControllerTests
  {
    private Mock<ILyricsService> lyricsServiceMock;

    private Mock<ILogger<LyricsController>> loggerMock;

    private LyricsController lyricsController;

    [SetUp]
    public void Setup()
    {
      lyricsServiceMock = new Mock<ILyricsService>(MockBehavior.Strict);

      loggerMock = new Mock<ILogger<LyricsController>>(MockBehavior.Loose);

      lyricsController = new LyricsController(
        lyricsServiceMock.Object,
        loggerMock.Object);
    }

    [Test]
    public async Task Get_WhenParamIsNull_ThrowsAnArgumentNullException()
    {
      // arrange
      string artistSlug = null;

      // act
      Func<Task> act = async () => await lyricsController.Get(artistSlug);

      // assert
      await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Get_WhenArtistDoesNotExist_ReturnsANotFoundResult()
    {
      // arrange
      string artistSlug = "John Doe";

      lyricsServiceMock
        .Setup(x => x.GetLyricsAsync(artistSlug))
        .ThrowsAsync(new ArtistNotFoundException(artistSlug));

      // act
      IActionResult result = await lyricsController.Get(artistSlug);

      // assert
      result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task Get_WhenArtistExist_ReturnsAListOfLyrics()
    {
      // arrange
      string artistSlug = "Fats Waller";

      string firstSongTitle = "Ain't Misbehavin'";
      string firstSongSlug = "aint-misbehavin";

      string secondSongTitle = "Write Myself a Letter";
      string secondSongSlug = "write-myself-a-letter";

      List<LyricCardViewModel> lyricsFromService = new List<LyricCardViewModel>
      {
        new LyricCardViewModel
        {
          Title = firstSongTitle,
          Slug = firstSongSlug
        },
        new LyricCardViewModel
        {
          Title = secondSongTitle,
          Slug = secondSongSlug
        }
      };

      lyricsServiceMock
        .Setup(x => x.GetLyricsAsync(artistSlug))
        .ReturnsAsync(lyricsFromService);

      // act
      IActionResult result = await lyricsController.Get(artistSlug);

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      List<LyricCardViewModel> lyrics = okObjectResult.Value as List<LyricCardViewModel>;

      lyrics.Should().NotBeEmpty();
      lyrics.Should().HaveCount(2);
      lyrics.First().Title.Should().Be(firstSongTitle);
      lyrics.First().Slug.Should().Be(firstSongSlug);
      lyrics.Last().Title.Should().Be(secondSongTitle);
      lyrics.Last().Slug.Should().Be(secondSongSlug);
    }
  }
}
