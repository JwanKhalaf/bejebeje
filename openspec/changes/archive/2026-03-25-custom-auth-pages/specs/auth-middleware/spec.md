## ADDED Requirements

### Requirement: Cookie-only authentication replacing OIDC middleware
The system SHALL replace the OpenID Connect authentication middleware with cookie-only authentication. The default scheme and default challenge scheme SHALL both be cookie authentication. The `LoginPath` SHALL be `/login`. The cookie SHALL be HTTP-only, secure (HTTPS-only), and SameSite=Lax.

#### Scenario: Unauthenticated user accessing protected endpoint
- **GIVEN** an unauthenticated user
- **WHEN** the user requests an `[Authorize]`-protected endpoint
- **THEN** the system redirects to `/login?ReturnUrl={originalUrl}`

#### Scenario: Cookie security flags
- **GIVEN** the cookie authentication is configured
- **WHEN** a cookie is issued
- **THEN** the cookie has `HttpOnly = true`, `SecurePolicy = Always`, and `SameSite = Lax`

### Requirement: OnSigningIn event handler for claim extraction and user sync
The system SHALL implement an `OnSigningInHandler` that runs during every `SignInAsync` call. It SHALL parse the ID token JWT from temporary claims, extract `sub`, `preferred_username`, and `email`, add them to the `ClaimsIdentity`, sync the local user record via `EnsureUserExistsAsync`, and ALWAYS strip `IdToken`, `AccessToken`, and `RefreshToken` claims in a `finally` path before the cookie is written.

#### Scenario: Successful claim extraction and user sync
- **GIVEN** a login produces a `ClaimsIdentity` with an `IdToken` claim containing a valid JWT
- **WHEN** the `OnSigningIn` event fires
- **THEN** `sub`, `preferred_username`, and `email` are extracted from the JWT and added to the identity, `EnsureUserExistsAsync` is called, and token claims are stripped

#### Scenario: Missing IdToken claim
- **GIVEN** the `ClaimsIdentity` has no `IdToken` claim
- **WHEN** the `OnSigningIn` event fires
- **THEN** a warning is logged, token claims are still stripped, and the cookie is issued without essential claims

#### Scenario: JWT parsing failure
- **GIVEN** the `IdToken` claim contains an invalid JWT
- **WHEN** the `OnSigningIn` event fires
- **THEN** the error is logged, token claims are stripped in the `finally` path, and the cookie is issued (login is not blocked)

#### Scenario: preferred_username missing from token — fallback to Cognito
- **GIVEN** the ID token JWT does not contain `preferred_username`
- **WHEN** the `OnSigningIn` event fires
- **THEN** the handler calls `ICognitoService.GetPreferredUsernameAsync(sub)` as a fallback and adds the result as a claim

#### Scenario: preferred_username fallback also fails
- **GIVEN** the ID token lacks `preferred_username` and the Cognito fallback fails
- **WHEN** the `OnSigningIn` event fires
- **THEN** a warning is logged, user sync is skipped (no username available), token claims are stripped, and the cookie is issued

#### Scenario: EnsureUserExistsAsync fails during OnSigningIn
- **GIVEN** `EnsureUserExistsAsync` throws an exception
- **WHEN** the `OnSigningIn` event fires
- **THEN** the error is logged but login is not blocked — the cookie is issued with claims

#### Scenario: EnsureUserExistsAsync returns null (duplicate username)
- **GIVEN** `EnsureUserExistsAsync` returns `null` due to a duplicate username collision
- **WHEN** the `OnSigningIn` event fires
- **THEN** a warning is logged but login proceeds normally

#### Scenario: Token claims are always stripped
- **GIVEN** any outcome of the `OnSigningIn` handler (success, failure, or skip)
- **WHEN** the cookie is about to be written
- **THEN** `IdToken`, `AccessToken`, and `RefreshToken` claims have been removed from the identity

### Requirement: IAuthService encapsulating Cognito SDK calls
The system SHALL provide an `IAuthService` interface with methods `AuthenticateAsync`, `SignUpAsync`, `ConfirmSignUpAsync`, `ResendConfirmationCodeAsync`, `ForgotPasswordAsync`, and `ConfirmForgotPasswordAsync`. The implementation SHALL catch Cognito exceptions and return typed result objects with error enums and user-facing messages. All methods SHALL compute and include the `SECRET_HASH`.

#### Scenario: AuthenticateAsync returns success with tokens
- **GIVEN** valid credentials
- **WHEN** `AuthenticateAsync` is called
- **THEN** it returns `AuthResult` with `Success = true` and `IdToken`, `AccessToken`, `RefreshToken` populated

#### Scenario: AuthenticateAsync maps NotAuthorizedException
- **GIVEN** Cognito throws `NotAuthorizedException`
- **WHEN** `AuthenticateAsync` is called
- **THEN** it returns `AuthResult` with `Success = false`, `ErrorType = InvalidCredentials`, and message "Invalid email or password. Please try again."

#### Scenario: SignUpAsync maps UsernameExistsException
- **GIVEN** Cognito throws `UsernameExistsException`
- **WHEN** `SignUpAsync` is called
- **THEN** it returns `SignUpResult` with `ErrorType = UsernameExists` and message "An account with this email already exists."

#### Scenario: ConfirmSignUpAsync maps CodeMismatchException
- **GIVEN** Cognito throws `CodeMismatchException`
- **WHEN** `ConfirmSignUpAsync` is called
- **THEN** it returns `ConfirmResult` with `ErrorType = CodeMismatch` and message "Invalid confirmation code. Please check the code and try again."

#### Scenario: ForgotPasswordAsync treats UserNotFoundException as success
- **GIVEN** Cognito throws `UserNotFoundException`
- **WHEN** `ForgotPasswordAsync` is called
- **THEN** it returns `ForgotPasswordResult` with `Success = true` (no information disclosure)

#### Scenario: SECRET_HASH is computed for every call
- **GIVEN** any `IAuthService` method is called
- **WHEN** the Cognito SDK request is built
- **THEN** the `SECRET_HASH` is set to HMAC-SHA256(email + clientId, clientSecret) base64-encoded

#### Scenario: Client IP forwarded as IPv4 only
- **GIVEN** `AuthenticateAsync` is called with a valid IPv4 `clientIp`
- **WHEN** the Cognito SDK request is built
- **THEN** `UserContextData.IpAddress` is set to the IPv4 address

#### Scenario: Non-IPv4 client IP is not forwarded
- **GIVEN** `AuthenticateAsync` is called with an IPv6 address or null
- **WHEN** the Cognito SDK request is built
- **THEN** `UserContextData` is not set

### Requirement: CognitoOptions typed configuration
The system SHALL provide a `CognitoOptions` class with properties `ClientId`, `ClientSecret`, `Authority`, `UserPoolId`, bound from the `Cognito` configuration section. All Cognito SDK calls SHALL use this options class.

#### Scenario: CognitoOptions bound from configuration
- **GIVEN** the `Cognito` section exists in app configuration
- **WHEN** the application starts
- **THEN** `CognitoOptions` is available via `IOptions<CognitoOptions>` with all properties populated

### Requirement: ICognitoService extended with GetUserByEmailAsync
The system SHALL add `GetUserByEmailAsync(string email)` to `ICognitoService` that calls Cognito `AdminGetUser` using the email as the username and returns a `CognitoUserInfo` record with `Sub` and `PreferredUsername`, or `null` if the user is not found.

#### Scenario: GetUserByEmailAsync returns user info
- **GIVEN** a confirmed user exists in Cognito with the given email
- **WHEN** `GetUserByEmailAsync` is called
- **THEN** it returns a `CognitoUserInfo` with the user's `Sub` and `PreferredUsername`

#### Scenario: GetUserByEmailAsync returns null for non-existent user
- **GIVEN** no user exists with the given email
- **WHEN** `GetUserByEmailAsync` is called
- **THEN** it returns `null`

### Requirement: CognitoService constructor refactored to IOptions
The `CognitoService` constructor SHALL change from `IConfiguration` to `IOptions<CognitoOptions>` for reading the `UserPoolId`.

#### Scenario: CognitoService uses CognitoOptions
- **GIVEN** `CognitoService` is instantiated
- **WHEN** any method is called
- **THEN** it reads `UserPoolId` from `IOptions<CognitoOptions>` (not raw `IConfiguration`)

## REMOVED Requirements

### Requirement: OpenID Connect middleware and Hosted UI
The OIDC middleware registration (`AddOpenIdConnect`), all OIDC-related using statements, the `Microsoft.AspNetCore.Authentication.OpenIdConnect` package dependency, and the `JwtSecurityTokenHandler` static configuration (`DefaultMapInboundClaims`, `DefaultInboundClaimTypeMap`) SHALL be removed from `Bejebeje.Mvc`.

#### Scenario: OIDC package removed
- **GIVEN** the updated `Bejebeje.Mvc.csproj`
- **WHEN** inspected
- **THEN** `Microsoft.AspNetCore.Authentication.OpenIdConnect` is not referenced

#### Scenario: No OIDC middleware in Program.cs
- **GIVEN** the updated `Program.cs`
- **WHEN** inspected
- **THEN** there is no `AddOpenIdConnect` call, no OIDC-related configuration, and no `JwtSecurityTokenHandler` static configuration

### Requirement: OnTokenValidatedHandler deleted
The `OnTokenValidatedHandler` class and its test file `OnTokenValidatedHandlerTests` SHALL be deleted. Their logic is migrated to `OnSigningInHandler`.

#### Scenario: OnTokenValidatedHandler files removed
- **GIVEN** the updated codebase
- **WHEN** the file system is inspected
- **THEN** `Bejebeje.Mvc/Auth/OnTokenValidatedHandler.cs` and `Bejebeje.Mvc.Tests/Auth/OnTokenValidatedHandlerTests.cs` do not exist
