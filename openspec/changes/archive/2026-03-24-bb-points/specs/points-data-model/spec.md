## ADDED Requirements

### Requirement: The system SHALL define a User entity for local user records with materialized point totals
A `User` entity SHALL be created in Bejebeje.Domain with the following fields: Id (int, PK, auto-increment), CognitoUserId (string, UNIQUE, NOT NULL), Username (string, UNIQUE, NOT NULL), ArtistSubmissionPoints (int, NOT NULL, DEFAULT 0), ArtistApprovalPoints (int, NOT NULL, DEFAULT 0), LyricSubmissionPoints (int, NOT NULL, DEFAULT 0), LyricApprovalPoints (int, NOT NULL, DEFAULT 0), ReportSubmissionPoints (int, NOT NULL, DEFAULT 0), ReportAcknowledgementPoints (int, NOT NULL, DEFAULT 0), LastSeenPoints (int, NOT NULL, DEFAULT 0), CreatedAt (DateTime, NOT NULL), ModifiedAt (DateTime?, NULL). The database table SHALL be named `users` with snake_case naming. UNIQUE indexes SHALL exist on `cognito_user_id` and `username`.

#### Scenario: User entity is persisted with all required fields
- **GIVEN** a new User entity is created with CognitoUserId "abc-123" and Username "testuser"
- **WHEN** the entity is saved to the database
- **THEN** a row exists in the `users` table with all point columns defaulting to 0, LastSeenPoints defaulting to 0, and CreatedAt set to the current UTC time

#### Scenario: Duplicate CognitoUserId is rejected
- **GIVEN** a User with CognitoUserId "abc-123" already exists
- **WHEN** another User with CognitoUserId "abc-123" is inserted
- **THEN** a unique constraint violation occurs

#### Scenario: Duplicate Username is rejected
- **GIVEN** a User with Username "testuser" already exists
- **WHEN** another User with Username "testuser" is inserted
- **THEN** a unique constraint violation occurs

### Requirement: The system SHALL define a PointEvent entity for individual point-earning events
A `PointEvent` entity SHALL be created in Bejebeje.Domain with the following fields: Id (int, PK, auto-increment), UserId (int, FK to User.Id, NOT NULL), ActionType (int, NOT NULL — PointActionType enum value), Points (int, NOT NULL), EntityId (int, NOT NULL), EntityName (string, NOT NULL), CreatedAt (DateTime, NOT NULL). The database table SHALL be named `point_events`. An index `ix_point_events_user_id_created_at` SHALL exist on (UserId, CreatedAt DESC). A UNIQUE index `ix_point_events_user_id_action_type_entity_id` SHALL exist on (UserId, ActionType, EntityId) to prevent duplicate awards.

#### Scenario: PointEvent is persisted with all required fields
- **GIVEN** a User with Id 1 exists
- **WHEN** a PointEvent is created with UserId=1, ActionType=3 (LyricSubmitted), Points=5, EntityId=42, EntityName="Song Title"
- **THEN** a row exists in the `point_events` table with the correct values and CreatedAt set to current UTC time

#### Scenario: Duplicate point event is prevented by unique constraint
- **GIVEN** a PointEvent exists with UserId=1, ActionType=3, EntityId=42
- **WHEN** another PointEvent with UserId=1, ActionType=3, EntityId=42 is inserted
- **THEN** the unique constraint on (user_id, action_type, entity_id) prevents the duplicate

#### Scenario: PointEvents are queryable by user in reverse chronological order
- **GIVEN** a User with Id 1 has 25 PointEvents with varying CreatedAt values
- **WHEN** PointEvents are queried for UserId=1 ordered by CreatedAt DESC with limit 20
- **THEN** the 20 most recent events are returned in descending order

### Requirement: The system SHALL define a PointActionType enum
A `PointActionType` enum SHALL be created in Bejebeje.Domain with the following values: ArtistSubmitted=1, ArtistApproved=2, LyricSubmitted=3, LyricApproved=4, ReportSubmitted=5, ReportAcknowledged=6.

#### Scenario: All action types have expected integer values
- **GIVEN** the PointActionType enum is defined
- **WHEN** each value is cast to int
- **THEN** ArtistSubmitted=1, ArtistApproved=2, LyricSubmitted=3, LyricApproved=4, ReportSubmitted=5, ReportAcknowledged=6

### Requirement: The system SHALL define fixed point value constants
Point values SHALL be defined as constants in code (not configurable). The values SHALL be: ArtistSubmittedNoPhoto=1, ArtistSubmittedWithPhoto=5, ArtistApprovedNoPhoto=9, ArtistApprovedWithPhoto=10, LyricSubmitted=5, LyricApproved=15, ReportSubmitted=1, ReportAcknowledged=4. Like-based points SHALL be computed as `floor(total_likes / 10)`.

#### Scenario: Point constants return correct values for each action
- **GIVEN** the point value constants are defined
- **WHEN** each constant is read
- **THEN** the values match the fixed point table: artist submitted no photo=1, with photo=5, approved no photo=9, with photo=10, lyric submitted=5, approved=15, report submitted=1, acknowledged=4

### Requirement: The system SHALL define contributor label thresholds as fixed constants
Contributor labels SHALL be derived from total points using fixed thresholds: 0-49="New Contributor", 50-199="Contributor", 200-499="Regular Contributor", 500+="Top Contributor". Thresholds and labels SHALL be defined as constants in code.

#### Scenario: Contributor label is correctly derived for each threshold
- **GIVEN** a user with 0 total points
- **WHEN** the contributor label is computed
- **THEN** the label is "New Contributor"

#### Scenario: Contributor label changes at boundary values
- **GIVEN** a user with exactly 50 total points
- **WHEN** the contributor label is computed
- **THEN** the label is "Contributor"

#### Scenario: Top Contributor label for high point totals
- **GIVEN** a user with 500 total points
- **WHEN** the contributor label is computed
- **THEN** the label is "Top Contributor"

### Requirement: The system SHALL define daily limit configuration bound from appsettings
A `BbPointsOptions` class with a `DailyLimits` property of type `DailyLimitsOptions` SHALL be created. `DailyLimitsOptions` SHALL have int properties: ArtistSubmissions (default 5), LyricSubmissions (default 10), ReportSubmissions (default 5). The options SHALL be bound from the `BbPoints` section of `appsettings.json` and registered via `builder.Services.Configure<BbPointsOptions>()`.

#### Scenario: Daily limits are read from configuration
- **GIVEN** appsettings.json contains `BbPoints:DailyLimits:ArtistSubmissions=3`
- **WHEN** BbPointsOptions is resolved from DI
- **THEN** DailyLimits.ArtistSubmissions equals 3

#### Scenario: Default daily limits apply when not configured
- **GIVEN** appsettings.json does not contain a BbPoints section
- **WHEN** BbPointsOptions is resolved from DI
- **THEN** DailyLimits.ArtistSubmissions defaults to 5, LyricSubmissions to 10, ReportSubmissions to 5
