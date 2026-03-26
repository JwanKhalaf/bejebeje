namespace Bejebeje.Mvc.Controllers
{
  using System.Diagnostics;
  using System.Threading.Tasks;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.AspNetCore.Mvc;
  using Models;

  public class HomeController : Controller
  {
    private readonly IHomepageService _homepageService;

    public HomeController(IHomepageService homepageService)
    {
      _homepageService = homepageService;
    }

    public async Task<IActionResult> Index()
    {
      bool isAuthenticated = User.Identity?.IsAuthenticated ?? false;

      var viewModel = await _homepageService.GetHomepageViewModelAsync(isAuthenticated);

      return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      ErrorViewModel viewModel = new ErrorViewModel
      {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
      };

      return View(viewModel);
    }
  }
}
