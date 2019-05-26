namespace Bejebeje.Services.Services.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.ViewModels.Search;

  public interface ISearchService
  {
    Task<IList<SearchResultViewModel>> SearchAsync(string searchTerm);
  }
}
