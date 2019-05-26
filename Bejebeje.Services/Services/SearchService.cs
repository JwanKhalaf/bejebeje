namespace Bejebeje.Services.Services
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.DataAccess.Context;
  using Bejebeje.Services.Services.Interfaces;
  using Bejebeje.ViewModels.Search;
  using Microsoft.Extensions.Logging;

  public class SearchService : ISearchService
  {
    private BbContext context;

    private ILogger logger;

    public SearchService(
      BbContext context,
      ILogger<SearchService> logger)
    {
      this.context = context;
      this.logger = logger;
    }

    public async Task<IList<SearchResultViewModel>> SearchAsync(string searchTerm)
    {
      List<SearchResultViewModel> searchResults = new List<SearchResultViewModel>();

      return searchResults;
    }
  }
}
