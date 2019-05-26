namespace Bejebeje.Api.Tests.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.Api.Controllers;
  using Bejebeje.ViewModels.Search;
  using FluentAssertions;
  using Microsoft.AspNetCore.Mvc;
  using NUnit.Framework;

  [TestFixture]
  public class SearchControllerTests
  {
    private SearchController searchController;

    [SetUp]
    public void Setup()
    {
      searchController = new SearchController();
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

      // act
      IActionResult result = await searchController.Search(searchTerm);

      // assert
      result.Should().BeOfType<OkObjectResult>();
    }
  }
}
