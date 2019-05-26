namespace Bejebeje.Api.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.Services.Services.Interfaces;
  using Bejebeje.ViewModels.Search;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;

  public class SearchController : ControllerBase
  {
    private ISearchService searchService;

    private ILogger logger;

    public SearchController(
      ISearchService searchService,
      ILogger<SearchController> logger)
    {
      this.searchService = searchService;
      this.logger = logger;
    }

    public async Task<IActionResult> Search(string searchTerm)
    {
      if (string.IsNullOrEmpty(searchTerm))
      {
        throw new ArgumentNullException(nameof(searchTerm));
      }

      IList<SearchResultViewModel> searchResults = await searchService.SearchAsync(searchTerm);

      return Ok(searchResults);
    }
  }
}
