namespace Bejebeje.Services.Services
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.Services.Services.Interfaces;
  using Bejebeje.ViewModels.Search;
  using Microsoft.Extensions.Logging;

  public class SearchService : ISearchService
  {
    private ILogger logger;

    public SearchService(ILogger<SearchService> logger)
    {
      this.logger = logger;
    }

    public Task<IList<SearchResultViewModel>> SearchAsync(string searchTerm)
    {
      throw new System.NotImplementedException();
    }
  }
}
