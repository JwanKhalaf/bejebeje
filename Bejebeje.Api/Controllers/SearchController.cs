namespace Bejebeje.Api.Controllers
{
  using System;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Mvc;

  public class SearchController : ControllerBase
  {
    public Task<IActionResult> Search(string searchTerm)
    {
      if (string.IsNullOrEmpty(searchTerm))
      {
        throw new ArgumentNullException(nameof(searchTerm));
      }

      throw new NotImplementedException();
    }
  }
}
