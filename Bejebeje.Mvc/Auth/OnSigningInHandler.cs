namespace Bejebeje.Mvc.Auth;

using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Bejebeje.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;

public class OnSigningInHandler
{
  private readonly IBbPointsService _bbPointsService;
  private readonly ICognitoService _cognitoService;
  private readonly ILogger<OnSigningInHandler> _logger;

  public OnSigningInHandler(
    IBbPointsService bbPointsService,
    ICognitoService cognitoService,
    ILogger<OnSigningInHandler> logger)
  {
    _bbPointsService = bbPointsService;
    _cognitoService = cognitoService;
    _logger = logger;
  }

  public async Task HandleAsync(ClaimsIdentity identity)
  {
    try
    {
      string idToken = identity.FindFirst("IdToken")?.Value;

      if (string.IsNullOrEmpty(idToken))
      {
        _logger.LogWarning("on signing in: IdToken claim is missing, skipping claim extraction");
        return;
      }

      // parse the jwt payload (manual base64url decode, no validation needed)
      string sub = null;
      string preferredUsername = null;
      string email = null;

      try
      {
        var parts = idToken.Split('.');

        if (parts.Length < 2)
        {
          _logger.LogWarning("on signing in: IdToken is not a valid jwt (expected 3 parts)");
          return;
        }

        string payload = parts[1];

        // base64url to base64
        payload = payload.Replace('-', '+').Replace('_', '/');

        switch (payload.Length % 4)
        {
          case 2: payload += "=="; break;
          case 3: payload += "="; break;
        }

        byte[] payloadBytes = Convert.FromBase64String(payload);
        string payloadJson = Encoding.UTF8.GetString(payloadBytes);

        using var doc = JsonDocument.Parse(payloadJson);
        var root = doc.RootElement;

        if (root.TryGetProperty("sub", out var subProp))
        {
          sub = subProp.GetString();
        }

        if (root.TryGetProperty("preferred_username", out var usernameProp))
        {
          preferredUsername = usernameProp.GetString();
        }

        if (root.TryGetProperty("email", out var emailProp))
        {
          email = emailProp.GetString();
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "on signing in: failed to parse IdToken jwt");
        return;
      }

      // add extracted claims to the identity
      if (!string.IsNullOrEmpty(sub))
      {
        identity.AddClaim(new Claim("sub", sub));
      }

      if (!string.IsNullOrEmpty(email))
      {
        identity.AddClaim(new Claim("email", email));
      }

      // handle preferred_username fallback
      if (string.IsNullOrEmpty(preferredUsername) && !string.IsNullOrEmpty(sub))
      {
        _logger.LogDebug("preferred_username not in token, falling back to cognito api for {CognitoUserId}", sub);

        try
        {
          preferredUsername = await _cognitoService.GetPreferredUsernameAsync(sub);
        }
        catch (Exception ex)
        {
          _logger.LogWarning(ex, "failed to resolve preferred_username from cognito for {CognitoUserId}, skipping user sync", sub);
          return;
        }
      }

      if (!string.IsNullOrEmpty(preferredUsername))
      {
        identity.AddClaim(new Claim("preferred_username", preferredUsername));
      }

      // user sync
      if (!string.IsNullOrEmpty(sub) && !string.IsNullOrEmpty(preferredUsername))
      {
        try
        {
          _logger.LogDebug("on signing in: syncing user record for {CognitoUserId}", sub);

          var user = await _bbPointsService.EnsureUserExistsAsync(sub, preferredUsername);

          if (user == null)
          {
            _logger.LogWarning("on signing in: EnsureUserExistsAsync returned null for {CognitoUserId} (possible duplicate username)", sub);
          }
          else
          {
            _logger.LogInformation("user record synced for {CognitoUserId} with username {Username}", sub, preferredUsername);
          }
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "on signing in: failed to sync user record for {CognitoUserId}, login will proceed", sub);
        }
      }
    }
    finally
    {
      // always strip token claims to keep cookie size small
      RemoveClaim(identity, "IdToken");
      RemoveClaim(identity, "AccessToken");
      RemoveClaim(identity, "RefreshToken");
    }
  }

  private static void RemoveClaim(ClaimsIdentity identity, string claimType)
  {
    var claim = identity.FindFirst(claimType);

    if (claim != null)
    {
      identity.RemoveClaim(claim);
    }
  }
}
