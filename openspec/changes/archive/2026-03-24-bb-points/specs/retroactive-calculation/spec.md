## ADDED Requirements

### Requirement: A standalone console application SHALL perform retroactive point calculation
A `Bejebeje.Retroactive` console application SHALL be created as a new project in the solution. It accepts a connection string via command-line argument or environment variable, connects to the database, and performs a one-time calculation of BB points for all existing users.

#### Scenario: Console app connects to database and processes all users
- **GIVEN** the Bejebeje.Retroactive app is run with a valid connection string
- **WHEN** it executes
- **THEN** it queries all distinct user_ids from artists, lyrics, lyric_reports, and likes tables, and processes each user

#### Scenario: Console app returns exit code 0 on success
- **GIVEN** the retroactive calculation completes without errors
- **WHEN** the process exits
- **THEN** the exit code is 0

#### Scenario: Console app returns non-zero exit code on failure
- **GIVEN** the retroactive calculation encounters an unrecoverable error
- **WHEN** the process exits
- **THEN** the exit code is non-zero

### Requirement: The retroactive calculation SHALL compute per-category points for each user
For each distinct user_id, the script SHALL compute: artist_submission_points as SUM(CASE WHEN has_image THEN 5 ELSE 1 END) from non-deleted artists, artist_approval_points as SUM(CASE WHEN has_image THEN 10 ELSE 9 END) from approved non-deleted artists, lyric_submission_points as 5 * COUNT from non-deleted lyrics, lyric_approval_points as 15 * COUNT from approved non-deleted lyrics, report_submission_points as 1 * COUNT from non-deleted lyric_reports, report_acknowledgement_points as 4 * COUNT from acknowledged non-deleted lyric_reports.

#### Scenario: Points computed correctly for user with mixed contributions
- **GIVEN** a user who submitted 3 artists (2 with photo, 1 without), all approved, 5 lyrics (3 approved), 2 reports (1 acknowledged)
- **WHEN** the retroactive calculation processes this user
- **THEN** artist_submission_points=11 (2*5+1*1), artist_approval_points=29 (2*10+1*9), lyric_submission_points=25 (5*5), lyric_approval_points=45 (3*15), report_submission_points=2, report_acknowledgement_points=4

#### Scenario: Deleted entities are excluded from calculation
- **GIVEN** a user who submitted 5 artists, 2 of which are soft-deleted (is_deleted=true)
- **WHEN** the retroactive calculation processes this user
- **THEN** only the 3 non-deleted artists are counted

### Requirement: The retroactive calculation SHALL resolve usernames from Cognito
For each distinct user_id, the script SHALL resolve the preferred_username via the Cognito AdminGetUser API. If the Cognito account is deleted or resolution fails, a fallback username of `"user-{first8charsOfUserId}"` SHALL be used.

#### Scenario: Username resolved from Cognito
- **GIVEN** a user_id "abc-def-123" that maps to a Cognito user with preferred_username "realuser"
- **WHEN** the retroactive calculation processes this user_id
- **THEN** the User record is created with Username="realuser"

#### Scenario: Fallback username for deleted Cognito account
- **GIVEN** a user_id "abc-def-456" that no longer exists in Cognito
- **WHEN** the retroactive calculation processes this user_id
- **THEN** the User record is created with Username="user-abc-def-"

### Requirement: The retroactive calculation SHALL UPSERT user records and set LastSeenPoints
The script SHALL use UPSERT (INSERT ON CONFLICT UPDATE) to create or update user records. LastSeenPoints SHALL be set equal to the computed total points to prevent false points-changed indicators on first login.

#### Scenario: New user record created with correct LastSeenPoints
- **GIVEN** no user record exists for user_id "xyz-789"
- **WHEN** the retroactive calculation processes this user with TotalPoints=150
- **THEN** a new User record is inserted with all category totals and LastSeenPoints=150

#### Scenario: Script is idempotent — running twice produces same result
- **GIVEN** the retroactive calculation has already run once
- **WHEN** it runs again
- **THEN** all User records have the same values (UPSERT overwrites with recalculated data)

### Requirement: The retroactive calculation SHALL NOT create point_events entries
No rows SHALL be inserted into the `point_events` table during retroactive calculation. The activity feed only records post-launch events.

#### Scenario: No point_events created during retroactive run
- **GIVEN** the retroactive calculation processes 50 users
- **WHEN** the calculation completes
- **THEN** the point_events table remains empty

### Requirement: Daily limits SHALL NOT apply to retroactive calculations
All historical actions SHALL earn their full point values regardless of how many occurred on a single day.

#### Scenario: Multiple same-day submissions all earn points retroactively
- **GIVEN** a user submitted 10 artists on a single day historically
- **WHEN** the retroactive calculation processes this user
- **THEN** all 10 artists contribute to artist_submission_points (no daily limit applied)

### Requirement: The retroactive calculation SHALL log progress and summary
The script SHALL log to stdout: progress during processing (e.g., users processed count), a final summary (total users processed, total points awarded), and any errors encountered.

#### Scenario: Progress and summary are logged
- **GIVEN** the retroactive calculation processes 25 users
- **WHEN** the calculation completes
- **THEN** stdout contains progress messages and a summary with: 25 users processed, total points awarded, and any errors

### Requirement: The retroactive script SHALL detect existing point_events and warn
If the `point_events` table contains data when the script runs, it SHALL warn the operator. If run after BB Points is live, the script must sum existing point_events per user and add them to retroactive totals to avoid overwriting post-launch data.

#### Scenario: Warning when point_events has data
- **GIVEN** the point_events table contains rows
- **WHEN** the retroactive calculation starts
- **THEN** a warning is logged indicating the script is running against a database with existing point events
