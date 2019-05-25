namespace Bejebeje.Api.Tests.Controllers
{
  using System;
  using System.Threading.Tasks;
  using Bejebeje.Api.Controllers;
  using FluentAssertions;
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
  }
}
