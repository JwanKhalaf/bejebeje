## ADDED Requirements

### Requirement: The system SHALL provide an IBbPointsService with EnsureUserExistsAsync
`IBbPointsService` SHALL expose `EnsureUserExistsAsync(string cognitoUserId, string username)` that returns a `User`. If no record exists for the cognitoUserId, it creates one with all point columns at 0. If the record exists but the username differs, it updates the username. Registered as scoped in the DI container.

#### Scenario: Create new user record
- **GIVEN** no user record exists for cognitoUserId "abc-123"
- **WHEN** EnsureUserExistsAsync("abc-123", "testuser") is called
- **THEN** a new User is returned with CognitoUserId="abc-123", Username="testuser", all point columns=0, LastSeenPoints=0

#### Scenario: Return existing user with matching username
- **GIVEN** a user record exists with CognitoUserId="abc-123" and Username="testuser"
- **WHEN** EnsureUserExistsAsync("abc-123", "testuser") is called
- **THEN** the existing User is returned with no modifications

#### Scenario: Update username on existing record
- **GIVEN** a user record exists with CognitoUserId="abc-123" and Username="oldname"
- **WHEN** EnsureUserExistsAsync("abc-123", "newname") is called
- **THEN** the User is returned with Username="newname" and ModifiedAt updated

### Requirement: The system SHALL provide AwardSubmissionPointsAsync with daily limit enforcement
`AwardSubmissionPointsAsync(string cognitoUserId, string username, PointActionType actionType, int entityId, string entityName, int points)` SHALL check the daily submission limit for the action type. If within the limit: insert a PointEvent, increment the relevant category total on the User record, and return true. If over the limit: return false without creating a PointEvent or incrementing totals. Daily limits count submissions from the source tables (artists, lyrics, lyric_reports) for the user on the current UTC day.

#### Scenario: Award points when within daily limit
- **GIVEN** a user has 2 artist submissions today and the daily limit is 5
- **WHEN** AwardSubmissionPointsAsync is called with ActionType=ArtistSubmitted, Points=5
- **THEN** a PointEvent is created, the User's ArtistSubmissionPoints is incremented by 5, and true is returned

#### Scenario: Skip points when daily limit exceeded
- **GIVEN** a user has 5 artist submissions today and the daily limit is 5
- **WHEN** AwardSubmissionPointsAsync is called with ActionType=ArtistSubmitted, Points=1
- **THEN** no PointEvent is created, the User's ArtistSubmissionPoints is not changed, and false is returned

#### Scenario: Daily limit counts from source table not point_events
- **GIVEN** a user made 5 artist submissions today but only 3 earned points (2 exceeded the limit)
- **WHEN** the daily count is checked for artist submissions
- **THEN** the count is 5 (all submissions from the artists table), not 3

#### Scenario: Point event insertion and user total update are atomic
- **GIVEN** a valid submission within the daily limit
- **WHEN** AwardSubmissionPointsAsync is called
- **THEN** the PointEvent insert and User category total increment occur in the same database transaction

### Requirement: The system SHALL provide AwardApprovalPointsAsync that always awards and is idempotent
`AwardApprovalPointsAsync(string cognitoUserId, string username, PointActionType actionType, int entityId, string entityName, int points)` SHALL always award points (no daily limit check). It inserts a PointEvent using INSERT ON CONFLICT DO NOTHING (or equivalent). If the insert succeeds, it increments the relevant category total. If the insert is skipped (duplicate), it does not increment. This makes the method safe to call multiple times for the same event.

#### Scenario: Award approval points for the first time
- **GIVEN** no PointEvent exists for UserId=1, ActionType=ArtistApproved, EntityId=42
- **WHEN** AwardApprovalPointsAsync is called with those values and Points=10
- **THEN** a PointEvent is created and User.ArtistApprovalPoints is incremented by 10

#### Scenario: Duplicate approval award is safely ignored
- **GIVEN** a PointEvent already exists for UserId=1, ActionType=ArtistApproved, EntityId=42
- **WHEN** AwardApprovalPointsAsync is called again with the same values
- **THEN** no new PointEvent is created and User.ArtistApprovalPoints is not changed

### Requirement: The system SHALL provide GetNavBarDataAsync for the nav bar View Component
`GetNavBarDataAsync(string cognitoUserId)` SHALL return a `NavBarPointsViewModel` containing: TotalPoints (sum of 6 category columns + computed like points), ContributorLabel (derived from TotalPoints), HasPointsChanged (TotalPoints > User.LastSeenPoints). Like points are computed as `floor(COUNT(*) FROM likes WHERE user_id = cognitoUserId / 10)`.

#### Scenario: Nav bar data for user with points
- **GIVEN** a user with ArtistSubmissionPoints=5, LyricSubmissionPoints=10, all other categories=0, and 15 likes given, LastSeenPoints=0
- **WHEN** GetNavBarDataAsync is called
- **THEN** TotalPoints=16 (5+10+floor(15/10)=1), ContributorLabel="New Contributor", HasPointsChanged=true

#### Scenario: Nav bar data when points have not changed
- **GIVEN** a user with TotalPoints=50 and LastSeenPoints=50
- **WHEN** GetNavBarDataAsync is called
- **THEN** HasPointsChanged=false

### Requirement: The system SHALL provide GetOwnProfileDataAsync for the own profile page
`GetOwnProfileDataAsync(string cognitoUserId)` SHALL return an `OwnProfileViewModel` with: per-category point totals (6 categories + like points), TotalPoints, ContributorLabel, and the 20 most recent PointEvents ordered by CreatedAt DESC. It SHALL also update User.LastSeenPoints to the current TotalPoints (dismissing the indicator).

#### Scenario: Own profile data with breakdown and activity
- **GIVEN** a user with ArtistSubmissionPoints=5, LyricApprovalPoints=15, 25 likes, and 3 PointEvents
- **WHEN** GetOwnProfileDataAsync is called
- **THEN** the response contains per-category totals, LikePoints=2, TotalPoints=22, ContributorLabel="New Contributor", 3 activity items, and LastSeenPoints is updated to 22

#### Scenario: Visiting own profile dismisses the points-changed indicator
- **GIVEN** a user with TotalPoints=100 and LastSeenPoints=50
- **WHEN** GetOwnProfileDataAsync is called
- **THEN** User.LastSeenPoints is updated to 100

### Requirement: The system SHALL provide GetPublicProfileDataAsync for public profiles
`GetPublicProfileDataAsync(string username)` SHALL look up the user by username. If found, return a `PublicProfileViewModel` with: TotalPoints, ContributorLabel, ArtistsSubmittedCount (from artists table where user_id matches and is_deleted=false), LyricsSubmittedCount (from lyrics table where user_id matches and is_deleted=false). If not found, return null.

#### Scenario: Public profile for existing user
- **GIVEN** a user "testuser" with TotalPoints=75, 3 artists submitted, 7 lyrics submitted
- **WHEN** GetPublicProfileDataAsync("testuser") is called
- **THEN** the response contains TotalPoints=75, ContributorLabel="Contributor", ArtistsSubmittedCount=3, LyricsSubmittedCount=7

#### Scenario: Public profile for non-existent user
- **GIVEN** no user record exists with Username="unknown"
- **WHEN** GetPublicProfileDataAsync("unknown") is called
- **THEN** null is returned

#### Scenario: Contribution counts include all submissions not just approved
- **GIVEN** a user with 5 artists submitted (3 approved, 2 pending) and 10 lyrics (8 approved, 2 pending)
- **WHEN** GetPublicProfileDataAsync is called
- **THEN** ArtistsSubmittedCount=5 and LyricsSubmittedCount=10

### Requirement: The system SHALL provide GetSubmitterPointsAsync for lyric detail pages
`GetSubmitterPointsAsync(string cognitoUserId)` SHALL return a `SubmitterPointsViewModel` with: TotalPoints, ContributorLabel, and Username. If no user record exists, return a fallback with TotalPoints=0, ContributorLabel="New Contributor", and resolve the username via ICognitoService.GetPreferredUsernameAsync.

#### Scenario: Submitter points for user with record
- **GIVEN** a user record exists with CognitoUserId="sub-123", Username="songwriter", TotalPoints=210
- **WHEN** GetSubmitterPointsAsync("sub-123") is called
- **THEN** TotalPoints=210, ContributorLabel="Regular Contributor", Username="songwriter"

#### Scenario: Submitter points fallback when no user record exists
- **GIVEN** no user record exists for CognitoUserId="old-user-789"
- **WHEN** GetSubmitterPointsAsync("old-user-789") is called
- **THEN** TotalPoints=0, ContributorLabel="New Contributor", Username is resolved from CognitoService

### Requirement: The system SHALL provide GetDailySubmissionCountAsync for internal use
`GetDailySubmissionCountAsync(string cognitoUserId, string entityTable, DateTime utcDate)` SHALL count submissions from the specified source table for the user on the given UTC date. Used internally by AwardSubmissionPointsAsync.

#### Scenario: Count today's artist submissions
- **GIVEN** a user created 3 artists today (UTC) and 2 yesterday
- **WHEN** GetDailySubmissionCountAsync is called for the artists table and today's date
- **THEN** the count is 3
