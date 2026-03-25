## Architecture Overview

The authentication architecture changes from OIDC middleware (redirect to Cognito Hosted UI) to cookie-only authentication with direct Cognito SDK calls. See SPEC.md §4 for the full system architecture diagram.

### Layer Responsibilities

- **Bejebeje.Mvc (Controller layer)**: `AccountController` handles HTTP endpoints, cookie creation/destruction (`SignInAsync`/`SignOutAsync`), redirects, TempData, and view model preparation. Does NOT contain Cognito SDK calls.
- **Bejebeje.Services (Service layer)**: `IAuthService` encapsulates all Cognito authentication SDK calls. `ICognitoService` extended with `GetUserByEmailAsync`. Both use `IOptions<CognitoOptions>` for configuration.
- **Bejebeje.Models (DTO layer)**: View models for each auth page. Result types (`AuthResult`, `SignUpResult`, etc.) for service return values.
- **Bejebeje.Mvc/Auth**: `OnSigningInHandler` replaces `OnTokenValidatedHandler`. Runs during cookie `OnSigningIn` event.

### Key Design Decisions

1. **Result objects over exceptions**: `IAuthService` catches Cognito SDK exceptions internally and returns typed result objects (`AuthResult`, `SignUpResult`, etc.) with error enums and user-facing messages. The controller never references AWS SDK exception types. See SPEC.md §6.1.

2. **Temporary token claims pattern**: On login, the controller creates a `ClaimsIdentity` with `IdToken`, `AccessToken`, and `RefreshToken` as temporary claims, then calls `SignInAsync`. The `OnSigningInHandler` parses the ID token JWT, extracts essential claims (`sub`, `preferred_username`, `email`), syncs the user, and **always** strips the large token claims in a `finally` path. See SPEC.md §7 Flow 1 + Flow 2.

3. **Manual JWT parsing**: The ID token JWT is parsed by base64url-decoding the payload segment and deserializing with `System.Text.Json`. No `JwtSecurityTokenHandler` or third-party JWT library. See SPEC.md §7 Flow 2 step 6.

4. **Dual user creation points**: Primary creation after email verification (via `GetUserByEmailAsync` + `EnsureUserExistsAsync`). Safety net on every login in `OnSigningIn`. Both are idempotent. See SPEC.md §7 Flow 4 step 5 and Flow 2 step 9.

5. **TempData for email confirmation flow**: The email is carried from sign-up POST to confirmation GET via `TempData["SignUpEmail"]`, not query strings. This prevents the page from being directly navigable and avoids email mismatch attacks. See SPEC.md §7 Flow 3-4.

6. **Client IP forwarding**: The controller extracts the client IP from `X-Forwarded-For` or `RemoteIpAddress` and passes it to `IAuthService.AuthenticateAsync`. Only valid IPv4 addresses are forwarded (Cognito limitation). The service does not depend on `IHttpContextAccessor`. See SPEC.md §6.1.

7. **Rate limiting via built-in middleware**: ASP.NET Core `AddRateLimiter` with fixed-window policies keyed by client IP. Applied via `[EnableRateLimiting]` attributes on POST actions. See SPEC.md §6.3.

### Data Flow

See SPEC.md §7 for all core flows (Login, OnSigningIn, Sign-up, Email Confirmation, Resend Code, Forgotten Password, Reset Password, Logout).

### Configuration

A new `CognitoOptions` class (`ClientId`, `ClientSecret`, `Authority`, `UserPoolId`) is bound from the `Cognito` configuration section. Both `AuthService` and `CognitoService` consume this via `IOptions<CognitoOptions>`. See SPEC.md §4 Component 4 and §5.

### Files Changed

See SPEC.md Appendix A for the complete file inventory (new, modified, deleted).
