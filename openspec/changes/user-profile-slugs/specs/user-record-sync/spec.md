## MODIFIED Requirements

### Requirement: EnsureUserExistsAsync SHALL trim username before storage on new user creation
When creating a new user, `EnsureUserExistsAsync` SHALL trim leading/trailing whitespace from the `preferredUsername` parameter before storing it in the `Username` column.

#### Scenario: New user with trailing whitespace has username trimmed
- **GIVEN** `EnsureUserExistsAsync` is called with `preferredUsername = "ali fm "`
- **WHEN** no user exists for the given `cognitoUserId`
- **THEN** the created user record SHALL have `Username = "ali fm"` (trimmed)

### Requirement: EnsureUserExistsAsync SHALL generate and store slug on new user creation
When creating a new user, `EnsureUserExistsAsync` SHALL generate a slug from the trimmed username using `NormalizeStringForUrl` with collision resolution, and store it in the `Slug` column.

#### Scenario: New user gets slug generated from trimmed username
- **GIVEN** `EnsureUserExistsAsync` is called with `preferredUsername = "ali fm "`
- **WHEN** the user record is created
- **THEN** the user SHALL have `Slug = "ali-fm"` (generated from trimmed username)

### Requirement: EnsureUserExistsAsync SHALL detect duplicate trimmed usernames on new user creation (FR-26)
Before creating a new user, `EnsureUserExistsAsync` SHALL check whether the trimmed username already exists in the `users` table for a different `CognitoUserId`. If it does, the service SHALL log a warning and return null without creating a user record.

#### Scenario: Duplicate trimmed username blocks new user creation
- **GIVEN** an existing user with `Username = "kardox"` AND `CognitoUserId = "user-26"`
- **WHEN** `EnsureUserExistsAsync` is called with `cognitoUserId = "user-91"` and `preferredUsername = "kardox "` (trims to `"kardox"`)
- **THEN** no new user record SHALL be created AND the method SHALL log a warning AND return null

#### Scenario: Cognito session succeeds despite blocked user creation
- **GIVEN** a new user whose trimmed username collides with an existing user
- **WHEN** `EnsureUserExistsAsync` returns null
- **THEN** the Cognito authentication session SHALL still succeed (it is independent) AND points features SHALL fall back to defaults (0 points, "New Contributor" label, no profile link)

### Requirement: EnsureUserExistsAsync SHALL compare trimmed values when detecting username changes
The username comparison for detecting changes SHALL compare the trimmed Cognito username against the stored username (which is already trimmed). This prevents unnecessary updates when the raw Cognito value has trailing whitespace but the trimmed value matches.

#### Scenario: No unnecessary update when trimmed values match
- **GIVEN** an existing user with stored `Username = "ali fm"` AND Cognito returns `preferredUsername = "ali fm "` (trailing space)
- **WHEN** `EnsureUserExistsAsync` runs
- **THEN** the trimmed comparison SHALL be `"ali fm" == "ali fm"` (equal) AND no update SHALL occur

### Requirement: EnsureUserExistsAsync SHALL regenerate slug on username change
When a returning user's trimmed Cognito username differs from the stored username, `EnsureUserExistsAsync` SHALL update the username to the new trimmed value and regenerate the slug with collision resolution.

#### Scenario: Username change triggers slug regeneration
- **GIVEN** an existing user with `Username = "ali fm"` and `Slug = "ali-fm"` whose Cognito username changes to `"ali music"`
- **WHEN** `EnsureUserExistsAsync` detects the change
- **THEN** the user SHALL have `Username = "ali music"`, `Slug = "ali-music"`, and `ModifiedAt` set to UTC now

### Requirement: EnsureUserExistsAsync SHALL detect duplicate trimmed usernames on username change (FR-27)
When an existing user's Cognito username changes, `EnsureUserExistsAsync` SHALL check whether the new trimmed username collides with another user's username. If it does, the username and slug SHALL NOT be updated and a warning SHALL be logged.

#### Scenario: Username change blocked by duplicate detection
- **GIVEN** an existing user with `Username = "old-name"` AND another user exists with `Username = "new-name"`
- **WHEN** `EnsureUserExistsAsync` detects a Cognito username change to `"new-name"`
- **THEN** the first user's `Username` and `Slug` SHALL remain unchanged AND a warning SHALL be logged

## ADDED Requirements

### Requirement: EnsureUserExistsAsync SHALL use empty-slug fallback for non-ASCII usernames
When `NormalizeStringForUrl` returns an empty string for a username, `EnsureUserExistsAsync` SHALL use `user-{cognitoUserId[..Min(8, cognitoUserId.Length)]}` as the base slug, with collision resolution applied.

#### Scenario: Fallback slug generated for emoji-only username
- **GIVEN** a username consisting entirely of emoji (e.g. only characters stripped by NormalizeStringForUrl) AND `cognitoUserId = "abcd1234-5678"`
- **WHEN** `EnsureUserExistsAsync` generates a slug
- **THEN** the base slug SHALL be `"user-abcd1234"` with collision resolution applied

### Requirement: Key logging events SHALL be emitted for observability
`EnsureUserExistsAsync` SHALL log: slug generation for new users (debug), slug regeneration on username change (info), duplicate username detection blocking creation or update (warning), empty slug fallback triggered (warning).

#### Scenario: Warning logged when duplicate username detected
- **GIVEN** a new user whose trimmed username collides with an existing user
- **WHEN** `EnsureUserExistsAsync` detects the collision
- **THEN** a warning-level log message SHALL be emitted identifying the colliding username and both CognitoUserIds

#### Scenario: Warning logged when empty slug fallback is used
- **GIVEN** a username that produces an empty slug from `NormalizeStringForUrl`
- **WHEN** the empty-slug fallback is triggered
- **THEN** a warning-level log message SHALL be emitted
