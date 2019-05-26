namespace Bejebeje.Services.Tests.Services
{
  using System.Collections;
  using System.Collections.Generic;
  using System.Threading.Tasks;
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

      searchService = new SearchService(loggerMock.Object);
    }


    [Test]
    public async Task SearchAsync_WhenNoResultsMatch_ReturnsAnEmptyListOfSearchResultViewModels()
    {
      // arrange
      string searchTerm = "queen";

      // act
      IList<SearchResultViewModel> results = await searchService.SearchAsync(searchTerm);

      // assert
      results.Should().NotBeNull();
      results.Should().HaveCount(0);
    }
  }
}
