namespace Bejebeje.Mvc.Auth;

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Bejebeje.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;

public class OnTokenValidatedHandler
{
  private readonly IBbPointsService _bbPointsService;
  private readonly ICognitoService _cognitoService;
  private readonly ILogger<OnTokenValidatedHandler> _logger;

  public OnTokenValidatedHandler(
    IBbPointsService bbPointsService,
    ICognitoService cognitoService,
    ILogger<OnTokenValidatedHandler> logger)
  {
    _bbPointsService = bbPointsService;
    _cognitoService = cognitoService;
    _logger = logger;
  }

  public async Task HandleAsync(ClaimsPrincipal principal)
  {
    try
    {
      string cognitoUserId = principal.FindFirstValue("sub");

      if (string.IsNullOrEmpty(cognitoUserId))
      {
        _logger.LogWarning("on token validated: sub claim is missing, skipping user sync");
        return;
      }

      _logger.LogDebug("on token validated: syncing user record for {CognitoUserId}", cognitoUserId);

      string preferredUsername = principal.FindFirstValue("preferred_username");

      if (string.IsNullOrEmpty(preferredUsername))
      {
        _logger.LogDebug("preferred_username claim not found, falling back to cognito api for {CognitoUserId}", cognitoUserId);

        try
        {
          preferredUsername = await _cognitoService.GetPreferredUsernameAsync(cognitoUserId);
        }
        catch (Exception ex)
        {
          _logger.LogWarning(ex, "failed to resolve preferred_username from cognito for {CognitoUserId}, skipping user sync", cognitoUserId);
          return;
        }
      }

      await _bbPointsService.EnsureUserExistsAsync(cognitoUserId, preferredUsername);

      _logger.LogInformation("user record synced for {CognitoUserId} with username {Username}", cognitoUserId, preferredUsername);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "failed to sync user record during token validation");
    }
  }
}
