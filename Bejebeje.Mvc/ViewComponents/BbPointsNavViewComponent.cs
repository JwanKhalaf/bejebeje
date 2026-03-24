namespace Bejebeje.Mvc.ViewComponents;

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Bejebeje.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

public class BbPointsNavViewComponent : ViewComponent
{
  private readonly IBbPointsService _bbPointsService;
  private readonly ILogger<BbPointsNavViewComponent> _logger;

  public BbPointsNavViewComponent(
    IBbPointsService bbPointsService,
    ILogger<BbPointsNavViewComponent> logger)
  {
    _bbPointsService = bbPointsService;
    _logger = logger;
  }

  public async Task<IViewComponentResult> InvokeAsync()
  {
    try
    {
      if (UserClaimsPrincipal?.Identity?.IsAuthenticated != true)
      {
        return Content(string.Empty);
      }

      string cognitoUserId = UserClaimsPrincipal.FindFirstValue("sub");

      if (string.IsNullOrEmpty(cognitoUserId))
      {
        _logger.LogWarning("authenticated user has no sub claim, skipping bb points nav");
        return Content(string.Empty);
      }

      _logger.LogDebug("fetching nav bar points for user {CognitoUserId}", cognitoUserId);

      var model = await _bbPointsService.GetNavBarDataAsync(cognitoUserId);

      return View(model);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "failed to load bb points nav bar data");
      return Content(string.Empty);
    }
  }
}
