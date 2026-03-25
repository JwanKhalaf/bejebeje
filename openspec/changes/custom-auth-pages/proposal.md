## Summary

Replace the AWS Cognito Hosted UI with custom, in-app authentication pages so users never leave the Bejebeje site to log in, sign up, or reset their password. All authentication flows are brought in-house using direct Cognito SDK calls with cookie-based session management.

## Why

Users are currently redirected away from Bejebeje to a generic Cognito-hosted page for all authentication. This creates a disjointed experience, prevents branding control, and limits customisation of error messages and flow. Replacing it with custom pages provides a seamless, branded authentication experience and simplifies the auth architecture from OIDC middleware to cookie-only authentication.

## What Changes

- **BREAKING**: Remove the OpenID Connect middleware, Hosted UI configuration, `JwtSecurityTokenHandler` static configuration, and the `Microsoft.AspNetCore.Authentication.OpenIdConnect` package from `Bejebeje.Mvc`
- **BREAKING**: Delete `OnTokenValidatedHandler` and its tests (logic migrated to new `OnSigningInHandler`)
- Add a new `AccountController` with custom pages for login, sign-up, email confirmation, forgotten password, reset password, and logout
- Add a new `IAuthService` / `AuthService` in the Services layer encapsulating all Cognito SDK calls (login, sign-up, confirm, resend code, forgot password, reset password)
- Add `CognitoOptions` typed configuration class bound from the `Cognito` config section
- Extend `ICognitoService` with `GetUserByEmailAsync` for post-verification user creation
- Refactor `CognitoService` constructor from `IConfiguration` to `IOptions<CognitoOptions>`
- Add `OnSigningInHandler` to parse ID token JWTs, extract claims, sync local user records, and strip large token claims from the cookie
- Add view models for all auth pages in `Bejebeje.Models/Account/`
- Add Razor views for all auth pages in `Bejebeje.Mvc/Views/Account/`
- Update `_Layout.cshtml` navigation to show Login/Sign-up (anonymous) or Logout (authenticated), and hide Profile for anonymous users
- Add rate limiting (2 req/min per IP) on login and sign-up POST endpoints
- Configure cookie authentication as both default scheme and default challenge scheme with `LoginPath = /login`

## Capabilities

### New Capabilities
- `auth-pages`: Custom authentication pages (login, sign-up, email confirmation, forgotten password, reset password, logout) with direct Cognito SDK integration
- `auth-middleware`: Cookie-only authentication middleware with OnSigningIn event handler for JWT parsing, claim extraction, and user sync
- `auth-rate-limiting`: Rate limiting on authentication POST endpoints

### Modified Capabilities
- `navigation`: Layout navigation updated with conditional auth links based on authentication state

## Impact

- **Bejebeje.Mvc**: `Program.cs` rewritten for auth middleware. New controller, handler, and views added. OIDC package removed. Layout modified
- **Bejebeje.Services**: New `IAuthService`/`AuthService` and `CognitoOptions`. `ICognitoService`/`CognitoService` extended with new method and constructor change
- **Bejebeje.Models**: New view model classes for auth pages
- **Bejebeje.Mvc.Tests**: `OnTokenValidatedHandlerTests` deleted, new `OnSigningInHandlerTests` added
- **External dependency**: AWS Cognito app client must have `ALLOW_USER_PASSWORD_AUTH` enabled before deployment
- **User impact**: All existing OIDC sessions invalidated on deployment; users must log in again via the new login page

## Scope

- All authentication UI pages and their backend flows
- Cookie authentication middleware configuration
- Cognito SDK integration for all auth operations
- Navigation conditional rendering
- Rate limiting on auth endpoints
- No database schema changes
- No changes to existing controllers, services, or views beyond: `_Layout.cshtml` (nav), `ICognitoService`/`CognitoService` (new method + constructor), `Program.cs` (auth middleware rewrite), `Bejebeje.Mvc.csproj` (OIDC package removal), and deletion of `OnTokenValidatedHandler` + its tests

## References

- [REQUIREMENTS.md](../../REQUIREMENTS.md)
- [SPEC.md](../../SPEC.md)
