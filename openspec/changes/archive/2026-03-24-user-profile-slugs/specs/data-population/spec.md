## ADDED Requirements

### Requirement: Retroactive console app SHALL add a project reference to Bejebeje.Common
`Bejebeje.Retroactive.csproj` SHALL include a `<ProjectReference>` to `Bejebeje.Common` to access the `NormalizeStringForUrl` extension method.

#### Scenario: Retroactive project references Bejebeje.Common
- **GIVEN** the `Bejebeje.Retroactive.csproj` file
- **WHEN** the project references are inspected
- **THEN** a `<ProjectReference Include="..\Bejebeje.Common\Bejebeje.Common.csproj" />` SHALL exist

### Requirement: Retroactive console app Phase 1 SHALL trim usernames before storage
The existing Phase 1 (user upsert during points calculation) SHALL trim the resolved Cognito username before storing it in the `username` column. The `slug` column SHALL be left as NULL in the Phase 1 upsert (slug is populated in Phase 2).

#### Scenario: Phase 1 upsert stores trimmed username
- **GIVEN** a Cognito user with `preferred_username = "ali fm "`
- **WHEN** Phase 1 upserts the user record
- **THEN** the stored `username` SHALL be `"ali fm"` (trimmed) AND `slug` SHALL be NULL

### Requirement: Retroactive console app SHALL have a Phase 2 for slug generation and username trimming sweep
After Phase 1, a new Phase 2 SHALL query all users ordered by `id ASC` and for each user: (a) attempt to trim the username if it has leading/trailing whitespace, skipping the trim if it would collide with another user's username; (b) generate a slug using `NormalizeStringForUrl` with collision resolution and empty-slug fallback; (c) update the user's `slug` and (if trimmed) `username` in the database.

#### Scenario: Phase 2 processes users in id order
- **GIVEN** users with ids 1, 2, 3 exist in the `users` table
- **WHEN** Phase 2 runs
- **THEN** users SHALL be processed in order: id 1, id 2, id 3

#### Scenario: Phase 2 trims username without collision
- **GIVEN** user 32 has `username = "ali fm "` AND no other user has `username = "ali fm"`
- **WHEN** Phase 2 processes user 32
- **THEN** `username` SHALL be updated to `"ali fm"` AND `slug` SHALL be set to `"ali-fm"`

#### Scenario: Phase 2 skips trim when it would collide (Kardox case)
- **GIVEN** user 26 has `username = "Kardox"` AND user 91 has `username = "Kardox "` (trailing space)
- **WHEN** Phase 2 processes user 91
- **THEN** user 91's `username` SHALL remain `"Kardox "` (untrimmed) AND a warning SHALL be logged AND user 91's slug SHALL be `"kardox-2"` (because user 26, processed first by id order, already holds `"kardox"`)

#### Scenario: Phase 2 generates slug with empty-slug fallback
- **GIVEN** a user whose username produces an empty string from `NormalizeStringForUrl` AND `cognitoUserId = "abcd1234-5678"`
- **WHEN** Phase 2 processes that user
- **THEN** the slug SHALL be `"user-abcd1234"` with collision resolution applied

#### Scenario: Phase 2 applies slug collision resolution
- **GIVEN** user 26 (id=26) has `username = "Kardox"` AND user 91 (id=91) has `username = "Kardox "` AND users are processed in id order
- **WHEN** Phase 2 runs
- **THEN** user 26 SHALL get slug `"kardox"` AND user 91 SHALL get slug `"kardox-2"`

### Requirement: Phase 2 SHALL be idempotent
Running Phase 2 multiple times SHALL produce the same result given the same data and processing order. If a user already has a slug, it SHALL be recalculated and overwritten.

#### Scenario: Idempotent re-run produces same slugs
- **GIVEN** Phase 2 has already run and all users have slugs
- **WHEN** Phase 2 is run again
- **THEN** all slugs SHALL be identical to the first run

### Requirement: Phase 2 SHALL print a summary
After processing all users, Phase 2 SHALL print: users processed, slugs generated, usernames trimmed, trimming collisions skipped.

#### Scenario: Summary output after Phase 2 completion
- **GIVEN** Phase 2 processes 112 users, trims 8 usernames, skips 1 collision
- **WHEN** Phase 2 completes
- **THEN** the console output SHALL include a summary with users processed (112), slugs generated (112), usernames trimmed (8), trim collisions skipped (1)

## MODIFIED Requirements

### Requirement: Retroactive Phase 1 upsert SQL SHALL include slug column
The Phase 1 upsert SQL statement SHALL include the `slug` column, setting it to NULL on insert and leaving it unchanged on conflict update. This ensures the column is acknowledged in the SQL even though Phase 2 populates it.

#### Scenario: Phase 1 upsert SQL includes slug column
- **GIVEN** the Phase 1 upsert SQL statement
- **WHEN** the SQL is inspected
- **THEN** the INSERT column list SHALL include `slug` with value NULL AND the ON CONFLICT UPDATE clause SHALL NOT overwrite an existing slug value
