## ADDED Requirements

### Requirement: The system SHALL sync user records on login via the OIDC OnTokenValidated event
On the OIDC `OnTokenValidated` event in `Program.cs`, the system SHALL extract `cognitoUserId` from the `sub` claim, obtain `preferredUsername` from token claims (with `profile` scope) or via `ICognitoService.GetPreferredUsernameAsync` as fallback, and call `IBbPointsService.EnsureUserExistsAsync(cognitoUserId, preferredUsername)`.

#### Scenario: New user logs in for the first time
- **GIVEN** no user record exists for CognitoUserId "new-user-123"
- **WHEN** the user authenticates and OnTokenValidated fires with sub="new-user-123" and preferred_username="newuser"
- **THEN** a new row is inserted into the `users` table with CognitoUserId="new-user-123", Username="newuser", all point columns at 0, and LastSeenPoints=0

#### Scenario: Existing user logs in with same username
- **GIVEN** a user record exists with CognitoUserId="existing-123" and Username="existinguser"
- **WHEN** the user authenticates with sub="existing-123" and preferred_username="existinguser"
- **THEN** no changes are made to the user record

#### Scenario: Existing user logs in with changed username
- **GIVEN** a user record exists with CognitoUserId="user-456" and Username="oldname"
- **WHEN** the user authenticates with sub="user-456" and preferred_username="newname"
- **THEN** the Username column is updated to "newname" and ModifiedAt is set to current UTC time

#### Scenario: Username obtained from token claims when profile scope is available
- **GIVEN** the OIDC configuration includes the `profile` scope
- **WHEN** a user authenticates and the token contains a `preferred_username` claim
- **THEN** the username is read from the claim without calling the Cognito API

#### Scenario: Username obtained from Cognito API as fallback
- **GIVEN** the token does not contain a `preferred_username` claim
- **WHEN** OnTokenValidated fires
- **THEN** the system calls `ICognitoService.GetPreferredUsernameAsync(cognitoUserId)` to resolve the username

### Requirement: The OIDC configuration SHALL request the profile scope
The OIDC configuration in `Program.cs` SHALL add the `profile` scope to the existing `openid` and `email` scopes so that `preferred_username` is available in token claims. If this causes issues (e.g., additional consent prompts), the fallback Cognito API call is sufficient.

#### Scenario: Profile scope is added to OIDC configuration
- **GIVEN** the OIDC middleware is configured
- **WHEN** the scopes are inspected
- **THEN** the scopes include "openid", "email", and "profile"
