## 1. Configuration & Shared Infrastructure

- [x] 1.1 Create `CognitoOptions` class in `Bejebeje.Services/Config/CognitoOptions.cs` with properties `ClientId` (string), `ClientSecret` (string), `Authority` (string), `UserPoolId` (string). Ref: SPEC §4 Component 4, §5.
- [x] 1.2 Register `CognitoOptions` in `Program.cs` by binding `builder.Services.Configure<CognitoOptions>(builder.Configuration.GetSection("Cognito"))`. Ref: SPEC §4 Component 4.
- [x] 1.3 Create `CognitoUserInfo` record/DTO in `Bejebeje.Services/Services/CognitoUserInfo.cs` with `string Sub` and `string PreferredUsername`. Ref: SPEC §6.2.

## 2. Auth Result Types

- [x] 2.1 Create `AuthErrorType` enum (`InvalidCredentials`, `UserNotConfirmed`, `TooManyRequests`, `Unexpected`) and `AuthResult` class (`bool Success`, `string? IdToken`, `string? AccessToken`, `string? RefreshToken`, `AuthErrorType? ErrorType`, `string? ErrorMessage`) in `Bejebeje.Services/Services/`. Ref: SPEC §6.1 AuthenticateAsync.
- [x] 2.2 Create `SignUpErrorType` enum (`UsernameExists`, `InvalidPassword`, `InvalidParameter`, `Unexpected`) and `SignUpResult` class (`bool Success`, `SignUpErrorType? ErrorType`, `string? ErrorMessage`). Ref: SPEC §6.1 SignUpAsync.
- [x] 2.3 Create `ConfirmErrorType` enum (`CodeMismatch`, `ExpiredCode`, `AliasExists`, `Unexpected`) and `ConfirmResult` class (`bool Success`, `ConfirmErrorType? ErrorType`, `string? ErrorMessage`). Ref: SPEC §6.1 ConfirmSignUpAsync.
- [x] 2.4 Create `ResendResult` class (`bool Success`, `string? ErrorMessage`). Ref: SPEC §6.1 ResendConfirmationCodeAsync.
- [x] 2.5 Create `ForgotPasswordResult` class (`bool Success`, `string? ErrorMessage`). Ref: SPEC §6.1 ForgotPasswordAsync.
- [x] 2.6 Create `ResetErrorType` enum (`CodeMismatch`, `ExpiredCode`, `InvalidPassword`, `UserNotFound`, `Unexpected`) and `ResetPasswordResult` class (`bool Success`, `ResetErrorType? ErrorType`, `string? ErrorMessage`). Ref: SPEC §6.1 ConfirmForgotPasswordAsync.

## 3. IAuthService Interface & Implementation

- [x] 3.1 Create `IAuthService` interface in `Bejebeje.Services/Services/Interfaces/IAuthService.cs` with methods: `AuthenticateAsync(string email, string password, string? clientIp)`, `SignUpAsync(string email, string username, string password)`, `ConfirmSignUpAsync(string email, string code)`, `ResendConfirmationCodeAsync(string email)`, `ForgotPasswordAsync(string email)`, `ConfirmForgotPasswordAsync(string email, string code, string newPassword)`. Ref: SPEC §6.1.
- [x] 3.2 Create `AuthService` class in `Bejebeje.Services/Services/AuthService.cs` with constructor taking `IAmazonCognitoIdentityProvider` and `IOptions<CognitoOptions>`. Implement private `ComputeSecretHash(string username)` method: HMAC-SHA256 of `(username + clientId)` using `clientSecret`, returned as base64. Ref: SPEC §6.1 Secret Hash Helper.
- [x] 3.3 Implement `AuthenticateAsync`: call Cognito `InitiateAuth` with `AuthFlow = USER_PASSWORD_AUTH`, parameters `USERNAME`, `PASSWORD`, `SECRET_HASH`. Set `UserContextData.IpAddress` if clientIp is valid IPv4. Map `NotAuthorizedException` → `InvalidCredentials`, `UserNotFoundException` → `InvalidCredentials`, `UserNotConfirmedException` → `UserNotConfirmed`, `TooManyRequestsException` → `TooManyRequests`, all other → `Unexpected`. Return tokens on success. Ref: SPEC §6.1 AuthenticateAsync, §7 Flow 1.
- [x] 3.4 Implement `SignUpAsync`: call Cognito `SignUp` with `Username = email`, `Password`, user attribute `preferred_username = username`, `SecretHash`. Map `UsernameExistsException`, `InvalidPasswordException`, `InvalidParameterException`, all other. Ref: SPEC §6.1 SignUpAsync.
- [x] 3.5 Implement `ConfirmSignUpAsync`: call Cognito `ConfirmSignUp` with `Username = email`, `ConfirmationCode`, `SecretHash`. Map `CodeMismatchException`, `ExpiredCodeException`, `AliasExistsException`, all other. Ref: SPEC §6.1 ConfirmSignUpAsync.
- [x] 3.6 Implement `ResendConfirmationCodeAsync`: call Cognito `ResendConfirmationCode` with `Username = email`, `SecretHash`. Return `ResendResult`. Ref: SPEC §6.1 ResendConfirmationCodeAsync.
- [x] 3.7 Implement `ForgotPasswordAsync`: call Cognito `ForgotPassword` with `Username = email`, `SecretHash`. Treat `UserNotFoundException` as success (security). Ref: SPEC §6.1 ForgotPasswordAsync.
- [x] 3.8 Implement `ConfirmForgotPasswordAsync`: call Cognito `ConfirmForgotPassword` with `Username = email`, `ConfirmationCode`, `Password = newPassword`, `SecretHash`. Map `CodeMismatchException`, `ExpiredCodeException`, `InvalidPasswordException`, `UserNotFoundException`, all other. Ref: SPEC §6.1 ConfirmForgotPasswordAsync.
- [x] 3.9 Register `IAuthService` / `AuthService` in DI in `Program.cs`. Ref: SPEC §4 Component 2.

## 4. ICognitoService Extension

- [x] 4.1 Add `GetUserByEmailAsync(string email)` method signature to `ICognitoService` interface. Returns `CognitoUserInfo?`. Ref: SPEC §6.2.
- [x] 4.2 Implement `GetUserByEmailAsync` in `CognitoService`: call `AdminGetUser` with `Username = email`, `UserPoolId` from `CognitoOptions`. Extract `sub` and `preferred_username` attributes. Return `null` on `UserNotFoundException`. Log and re-throw other exceptions. Ref: SPEC §6.2.
- [x] 4.3 Refactor `CognitoService` constructor from `IConfiguration` to `IOptions<CognitoOptions>`. Update `UserPoolId` reads to use the options class. Update DI registration in `Program.cs` if needed. Ref: SPEC §6.2.

## 5. View Models

- [x] 5.1 Create `LoginViewModel` in `Bejebeje.Models/Account/LoginViewModel.cs` with properties: `Email` (string), `Password` (string), `RememberMe` (bool), `ReturnUrl` (string?), `ErrorMessage` (string?). Ref: SPEC §6.4.
- [x] 5.2 Create `SignupViewModel` in `Bejebeje.Models/Account/SignupViewModel.cs` with properties: `Email` (string), `Username` (string), `Password` (string), `ErrorMessage` (string?). Ref: SPEC §6.4.
- [x] 5.3 Create `ConfirmViewModel` in `Bejebeje.Models/Account/ConfirmViewModel.cs` with properties: `Email` (string), `Code` (string), `ErrorMessage` (string?), `SuccessMessage` (string?). Ref: SPEC §6.4.
- [x] 5.4 Create `ForgottenPasswordViewModel` in `Bejebeje.Models/Account/ForgottenPasswordViewModel.cs` with properties: `Email` (string), `ErrorMessage` (string?). Ref: SPEC §6.4.
- [x] 5.5 Create `ResetPasswordViewModel` in `Bejebeje.Models/Account/ResetPasswordViewModel.cs` with properties: `Email` (string), `Code` (string), `NewPassword` (string), `ConfirmPassword` (string), `ErrorMessage` (string?). Ref: SPEC §6.4.

## 6. OnSigningIn Handler

- [x] 6.1 Create `OnSigningInHandler` class in `Bejebeje.Mvc/Auth/OnSigningInHandler.cs` with dependencies: `IBbPointsService`, `ICognitoService`, `ILogger<OnSigningInHandler>`. Ref: SPEC §4 Component 5.
- [x] 6.2 Implement JWT parsing in `OnSigningInHandler`: split ID token by `.`, base64url-decode payload, deserialize with `System.Text.Json`, extract `sub`, `preferred_username`, `email`. Ref: SPEC §7 Flow 2 steps 4-6.
- [x] 6.3 Implement claim addition: add `sub`, `preferred_username`, `email` claims to the `ClaimsIdentity`. Ref: SPEC §7 Flow 2 step 7.
- [x] 6.4 Implement `preferred_username` fallback: if missing from token, call `ICognitoService.GetPreferredUsernameAsync(sub)`. Ref: SPEC §7 Flow 2 step 8.
- [x] 6.5 Implement user sync: call `IBbPointsService.EnsureUserExistsAsync(sub, preferredUsername)` when `sub` and username are available. Handle `null` return (warning) and exceptions (error). Ref: SPEC §7 Flow 2 step 9.
- [x] 6.6 Implement `finally` token stripping: always remove `IdToken`, `AccessToken`, `RefreshToken` claims from the identity regardless of success/failure. Ref: SPEC §7 Flow 2 step 10.
- [x] 6.7 Ensure all exceptions in steps 6.2-6.5 are caught, logged, and swallowed — login must never be blocked. Ref: SPEC §9 "OnSigningIn JWT parsing fails".

## 7. AccountController — Login

- [x] 7.1 Create `AccountController` in `Bejebeje.Mvc/Controllers/AccountController.cs` with `[AllowAnonymous]` on the class. Inject `IAuthService`, `IBbPointsService`, `ICognitoService`, `ILogger<AccountController>`. Ref: SPEC §4 Component 1.
- [x] 7.2 Implement `Login()` GET action mapped to `/login`. Render `Login.cshtml` with empty `LoginViewModel`. Populate `ReturnUrl` from query parameter. Ref: SPEC §7 Flow 1 steps 1-2.
- [x] 7.3 Implement `Login(LoginViewModel)` POST action mapped to `/login` with `[ValidateAntiForgeryToken]`. Validate model state. Extract client IP from `X-Forwarded-For` (first entry) or `RemoteIpAddress`, validate IPv4. Call `IAuthService.AuthenticateAsync(email, password, clientIp)`. Ref: SPEC §7 Flow 1 steps 4a-4d.
- [x] 7.4 On failed auth: log at appropriate level (Warning for known errors, Error for unexpected). Set `ErrorMessage` on view model, re-render with email retained. Ref: SPEC §7 Flow 1 step 5.
- [x] 7.5 On successful auth: create `ClaimsIdentity` with temporary `IdToken`, `AccessToken`, `RefreshToken` claims. Create `AuthenticationProperties` with `IsPersistent` and `ExpiresUtc` based on `RememberMe`. Call `SignInAsync`. Ref: SPEC §7 Flow 1 step 6.
- [x] 7.6 After `SignInAsync`: validate `ReturnUrl` via `Url.IsLocalUrl()`. Redirect to `ReturnUrl` if valid, otherwise `/`. Log successful login at Information level (email only). Ref: SPEC §7 Flow 1 steps 8-9.

## 8. AccountController — Sign-up

- [x] 8.1 Implement `Signup()` GET action mapped to `/signup`. Render `Signup.cshtml` with empty `SignupViewModel`. Ref: SPEC §7 Flow 3 steps 1-2.
- [x] 8.2 Implement `Signup(SignupViewModel)` POST action mapped to `/signup` with `[ValidateAntiForgeryToken]`. Validate model state. Call `IAuthService.SignUpAsync(email, username, password)`. Ref: SPEC §7 Flow 3 step 4.
- [x] 8.3 On failed sign-up: set `ErrorMessage`, re-render. On success: set `TempData["SignUpEmail"] = email`, redirect to `/signup/confirm`. Ref: SPEC §7 Flow 3 steps 5-6.

## 9. AccountController — Email Confirmation

- [x] 9.1 Implement `Confirm()` GET action mapped to `/signup/confirm`. Check `TempData["SignUpEmail"]` — if absent, redirect to `/signup`. If present, render `Confirm.cshtml` with `Email` pre-populated and in a hidden field. Ref: SPEC §7 Flow 4 step 1.
- [x] 9.2 Implement `Confirm(ConfirmViewModel)` POST action mapped to `/signup/confirm` with `[ValidateAntiForgeryToken]`. Read email from posted model (hidden field). Call `IAuthService.ConfirmSignUpAsync(email, code)`. Ref: SPEC §7 Flow 4 steps 2-3.
- [x] 9.3 On failed confirmation: set `ErrorMessage`, re-render. On success: attempt local user creation — call `ICognitoService.GetUserByEmailAsync(email)`, then `IBbPointsService.EnsureUserExistsAsync(sub, preferredUsername)`. Handle failures gracefully (log, continue). Redirect to `/login` with `TempData["ConfirmationSuccess"] = true`. Ref: SPEC §7 Flow 4 steps 4-5.
- [x] 9.4 Implement `ResendCode(ConfirmViewModel)` POST action mapped to `/signup/confirm/resend` with `[ValidateAntiForgeryToken]`. Read email from posted model. Call `IAuthService.ResendConfirmationCodeAsync(email)`. Set `SuccessMessage = "A new code has been sent to your email."`. Re-render `Confirm.cshtml` with email still populated. Ref: SPEC §7 Flow 5.

## 10. AccountController — Forgotten Password & Reset Password

- [x] 10.1 Implement `ForgottenPassword()` GET action mapped to `/forgotten-password`. Render `ForgottenPassword.cshtml`. Ref: SPEC §7 Flow 6 steps 1-2.
- [x] 10.2 Implement `ForgottenPassword(ForgottenPasswordViewModel)` POST action mapped to `/forgotten-password` with `[ValidateAntiForgeryToken]`. Call `IAuthService.ForgotPasswordAsync(email)`. Always redirect to `/reset-password?email={email}` (no information disclosure). Ref: SPEC §7 Flow 6 steps 3-5.
- [x] 10.3 Implement `ResetPassword()` GET action mapped to `/reset-password`. Render `ResetPassword.cshtml` with `Email` pre-populated from query string. Ref: SPEC §7 Flow 7 steps 1-2.
- [x] 10.4 Implement `ResetPassword(ResetPasswordViewModel)` POST action mapped to `/reset-password` with `[ValidateAntiForgeryToken]`. Server-side validate password == confirm password. Call `IAuthService.ConfirmForgotPasswordAsync(email, code, newPassword)`. On failure: set `ErrorMessage`, re-render. On success: redirect to `/login`. Ref: SPEC §7 Flow 7 steps 3-6.

## 11. AccountController — Logout

- [x] 11.1 Implement `Logout()` GET action mapped to `/logout`. Call `HttpContext.SignOutAsync()`. Redirect to `/login`. Do not require authentication. Ref: SPEC §7 Flow 8.

## 12. Razor Views

- [x] 12.1 Create `Views/Account/Login.cshtml`: email field, password field, "Remember me" checkbox, hidden `ReturnUrl` field, submit button, error message display, link to `/signup`, link to `/forgotten-password`, anti-forgery token. Use existing Tailwind CSS patterns. Ref: SPEC §6.4, §7 Flow 1.
- [x] 12.2 Create `Views/Account/Signup.cshtml`: email field, username field, password field, client-side password validation checklist (14+ chars, 1 number, 1 uppercase, 1 lowercase), submit button disabled until criteria met, error message display, link to `/login`, anti-forgery token. Ref: SPEC §6.4, §7 Flow 3.
- [x] 12.3 Create `Views/Account/Confirm.cshtml`: hidden email field (from TempData), confirmation code field, submit button, "Resend code" button (posts to `/signup/confirm/resend`), error/success message display, anti-forgery token. Ref: SPEC §6.4, §7 Flow 4-5.
- [x] 12.4 Create `Views/Account/ForgottenPassword.cshtml`: email field, submit button, error message display, anti-forgery token. Ref: SPEC §6.4, §7 Flow 6.
- [x] 12.5 Create `Views/Account/ResetPassword.cshtml`: email field (pre-populated from query string), code field, new password field, confirm password field, client-side validation checklist (password policy + match), submit button disabled until criteria met, error message display, anti-forgery token. Ref: SPEC §6.4, §7 Flow 7.
- [x] 12.6 Ensure all auth views are responsive (mobile + desktop) using existing Tailwind CSS patterns. Ref: SPEC §8 Accessibility.

## 13. Navigation Updates

- [x] 13.1 Modify `Views/Shared/_Layout.cshtml`: wrap Profile `<li>` in `@if (User.Identity?.IsAuthenticated == true)`. Ref: SPEC §6.5 item 1.
- [x] 13.2 Add Login `<li>` visible when `User.Identity?.IsAuthenticated != true`, linking to `/login`. Ref: SPEC §6.5 item 2.
- [x] 13.3 Add Sign-up `<li>` visible when `User.Identity?.IsAuthenticated != true`, linking to `/signup`. Ref: SPEC §6.5 item 3.
- [x] 13.4 Add Logout `<li>` visible when `User.Identity?.IsAuthenticated == true`, linking to `/logout`. Ref: SPEC §6.5 item 4.

## 14. Rate Limiting

- [x] 14.1 Add rate limiter middleware in `Program.cs`: call `builder.Services.AddRateLimiter()` with two fixed-window policies — `"auth-login"` and `"auth-signup"` — each allowing 2 requests per 1-minute window, keyed by client IP (`RemoteIpAddress`). Set `RejectionStatusCode = 429`. Ref: SPEC §6.3.
- [x] 14.2 Add `app.UseRateLimiter()` to the pipeline after `UseRouting()` and before `UseAuthentication()`. Ref: SPEC §6.3.
- [x] 14.3 Apply `[EnableRateLimiting("auth-login")]` to the Login POST action and `[EnableRateLimiting("auth-signup")]` to the Sign-up POST action. Ref: SPEC §6.3.

## 15. Program.cs — Auth Middleware Replacement

- [x] 15.1 Remove the `AddOpenIdConnect` middleware registration, all OIDC-related `using` statements, and the `JwtSecurityTokenHandler` static configuration (`DefaultMapInboundClaims`, `DefaultInboundClaimTypeMap`) from `Program.cs`. Ref: SPEC §4 Component 6, REQUIREMENTS FR-3, FR-4.
- [x] 15.2 Configure cookie-only authentication: set default scheme and default challenge scheme to cookies. Set `LoginPath = "/login"`. Set `HttpOnly = true`, `SecurePolicy = Always`, `SameSite = Lax`. Ref: SPEC §4 Component 1, §8 Security.
- [x] 15.3 Wire the `OnSigningIn` event in cookie authentication options to invoke `OnSigningInHandler`. Ref: SPEC §4 Component 5.
- [x] 15.4 Remove `Microsoft.AspNetCore.Authentication.OpenIdConnect` package reference from `Bejebeje.Mvc.csproj`. Ref: SPEC Appendix A.

## 16. Cleanup — Deleted Files

- [x] 16.1 Delete `Bejebeje.Mvc/Auth/OnTokenValidatedHandler.cs`. Ref: SPEC Appendix A.
- [x] 16.2 Delete `Bejebeje.Mvc.Tests/Auth/OnTokenValidatedHandlerTests.cs`. Ref: SPEC Appendix A.

## 17. Tests

- [x] 17.1 Create `Bejebeje.Mvc.Tests/Auth/OnSigningInHandlerTests.cs` with tests covering: successful claim extraction from JWT, missing IdToken, JWT parsing failure, `preferred_username` fallback, `EnsureUserExistsAsync` failure (login not blocked), `EnsureUserExistsAsync` returning null (duplicate username), token claims always stripped in finally path. Ref: SPEC §7 Flow 2, §9.
- [x] 17.2 Verify all existing tests pass after changes (`dotnet test`). No existing controller or service tests should break since no existing controllers/services are modified (except `CognitoService` constructor change). Ref: SPEC §8, REQUIREMENTS NFR-12.

## 18. Observability & Security Verification

- [x] 18.1 Verify all auth log statements follow the pattern: Information for successful logins (email only, never password), Warning for failed attempts and missing claims, Error for unexpected exceptions. Ref: SPEC §8 Observability.
- [x] 18.2 Verify passwords are never logged in any log statement across `AuthService`, `AccountController`, and `OnSigningInHandler`. Ref: SPEC §8 Security, REQUIREMENTS NFR-4.
- [x] 18.3 Verify error messages shown to users never leak Cognito exception details — only the pre-defined user-facing messages from SPEC §6.1 are used. Ref: SPEC §8 Security, REQUIREMENTS NFR-5.

## 19. Pre-Deployment Configuration

- [x] 19.1 Verified: `ALLOW_USER_PASSWORD_AUTH` enabled on the Cognito app client. Ref: SPEC §3 Constraint 1, §10.
- [x] 19.2 Verified: `preferred_username` is a required attribute and writable during sign-up. Ref: SPEC §3 Assumption 3, §10.
- [x] 19.3 Verified: Cognito user pool sign-in option is Email — `AdminGetUser` by email works. No changes needed. Ref: SPEC §3 Assumption 6, §11 Open Question 1.

## 20. AWSSDK Version Alignment

- [x] 20.1 Both projects aligned to `AWSSDK.CognitoIdentityProvider` v4.0.4 and `AWSSDK.Core` v4.0.1.3. All methods compile and tests pass. Ref: SPEC §3 Constraint 6.
