namespace Bejebeje.Mvc.Controllers
{
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  public class ProfileController : Controller
  {
    [Authorize]
    public IActionResult Index()
    {
      return View();
    }
  }
}
