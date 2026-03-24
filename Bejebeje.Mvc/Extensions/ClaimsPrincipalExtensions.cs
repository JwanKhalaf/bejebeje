namespace Bejebeje.Mvc.Extensions
{
  using System;
  using System.Security.Claims;

  public static class ClaimsPrincipalExtensions
  {
    public static Guid GetUserId(this ClaimsPrincipal user)
      => Guid.TryParse(user.FindFirstValue("sub"), out var id) ? id : Guid.Empty;

    public static string GetCognitoUserId(this ClaimsPrincipal user)
      => user.FindFirstValue("sub") ?? string.Empty;

    public static string GetPreferredUsername(this ClaimsPrincipal user)
      => user.FindFirstValue("preferred_username") ?? string.Empty;
  }
}
