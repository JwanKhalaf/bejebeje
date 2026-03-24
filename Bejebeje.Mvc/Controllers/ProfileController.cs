namespace Bejebeje.Mvc.Controllers;

using System.Threading.Tasks;
using Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Services.Interfaces;

public class ProfileController : Controller
{
  private readonly IBbPointsService _bbPointsService;
  private readonly ILogger<ProfileController> _logger;

  public ProfileController(
    IBbPointsService bbPointsService,
    ILogger<ProfileController> logger)
  {
    _bbPointsService = bbPointsService;
    _logger = logger;
  }

  [Authorize]
  [Route("profile")]
  public async Task<IActionResult> Index()
  {
    string cognitoUserId = User.GetCognitoUserId();

    _logger.LogDebug("loading own profile for user {CognitoUserId}", cognitoUserId);

    var model = await _bbPointsService.GetOwnProfileDataAsync(cognitoUserId);

    return View(model);
  }

  [Route("profile/{slug}")]
  public async Task<IActionResult> Public(string slug)
  {
    _logger.LogDebug("loading public profile for slug {Slug}", slug);

    var model = await _bbPointsService.GetPublicProfileDataAsync(slug);

    if (model == null)
    {
      return NotFound();
    }

    // if the authenticated user is viewing their own public profile url, redirect to own profile
    if (User.Identity?.IsAuthenticated == true)
    {
      string cognitoUserId = User.GetCognitoUserId();

      if (!string.IsNullOrEmpty(cognitoUserId) &&
          string.Equals(model.CognitoUserId, cognitoUserId, System.StringComparison.Ordinal))
      {
        return RedirectToAction("Index");
      }
    }

    return View(model);
  }
}
