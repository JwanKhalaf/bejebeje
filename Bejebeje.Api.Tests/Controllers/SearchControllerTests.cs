namespace Bejebeje.Api.Tests.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Api.Controllers;
  using Bejebeje.Common.Enums;
  using Bejebeje.Services.Services.Interfaces;
  using Bejebeje.ViewModels.Search;
  using FluentAssertions;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class SearchControllerTests
  {
    private Mock<ISearchService> searchServiceMock;

    private Mock<ILogger<SearchController>> loggerMock;

    private SearchController searchController;

    [SetUp]
    public void Setup()
    {
      searchServiceMock = new Mock<ISearchService>(MockBehavior.Strict);

      loggerMock = new Mock<ILogger<SearchController>>(MockBehavior.Loose);

      searchController = new SearchController(
        searchServiceMock.Object,
        loggerMock.Object);
    }

    [Test]
    public async Task Search_WhenParamIsNull_ThrowsArgumentNullException()
    {
      // arrange
      string searchTerm = null;

      // act
      Func<Task> action = async () => await searchController.Search(searchTerm);

      // assert
      await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Search_WhenParamIsEmpty_ThrowsArgumentNullException()
    {
      // arrange
      string searchTerm = "";

      // act
      Func<Task> action = async () => await searchController.Search(searchTerm);

      // assert
      await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Search_WhenNoResultsExist_ReturnsAnEmptyListOfSearchResultViewModel()
    {
      // arrange
      string searchTerm = "james brown";

      searchServiceMock
        .Setup(x => x.SearchAsync(searchTerm))
        .ReturnsAsync(new List<SearchResultViewModel>());

      // act
      IActionResult result = await searchController.Search(searchTerm);

      // assert
      result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Search_WhenResultsExist_ReturnsAPopulatedListOfSearchResultViewModel()
    {
      // arrange
      string searchTerm = "fats waller";
      string expectedSlugInResult = "fats-waller";
      string expectedNameInResult = "Fats Waller";
      int expectedImageIdInResult = 1;
      ResultType expectedResultType = ResultType.Artist;

      searchServiceMock
        .Setup(x => x.SearchAsync(searchTerm))
        .ReturnsAsync(new List<SearchResultViewModel>
        {
          new SearchResultViewModel
          {
            Slug = expectedSlugInResult,
            Name = expectedNameInResult,
            ImageId = expectedImageIdInResult,
            ResultType = expectedResultType
          }
        });

      // act
      IActionResult result = await searchController.Search(searchTerm);

      // assert
      result.Should().BeOfType<OkObjectResult>();

      OkObjectResult okObjectResult = result as OkObjectResult;

      okObjectResult.Should().NotBeNull();

      List<SearchResultViewModel> searchResults = okObjectResult.Value as List<SearchResultViewModel>;

      searchResults.Should().NotBeNull();
      searchResults.Should().HaveCount(1);
      searchResults.First().Slug.Should().Be(expectedSlugInResult);
      searchResults.First().Name.Should().Be(expectedNameInResult);
      searchResults.First().ImageId.Should().Be(expectedImageIdInResult);
      searchResults.First().ResultType.Should().Be(expectedResultType);
    }
  }
}
