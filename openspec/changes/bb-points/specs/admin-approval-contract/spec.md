## ADDED Requirements

### Requirement: The admin project SHALL award artist approval points when approving an artist
When an admin approves an artist in bejebeje.admin, the admin project SHALL: look up the artist's user_id, find or create a users record (resolve username from Cognito if creating), determine points (10 if has_image=true, 9 if false), INSERT into point_events with ActionType=ArtistApproved (skip if duplicate via unique constraint), increment users.artist_approval_points, and send an approval notification email to the submitter.

#### Scenario: Artist with photo approved earns submitter 10 points
- **GIVEN** an artist with has_image=true submitted by user "submitter1"
- **WHEN** an admin approves the artist
- **THEN** a PointEvent with ActionType=ArtistApproved, Points=10 is created, and the user's artist_approval_points is incremented by 10

#### Scenario: Artist without photo approved earns submitter 9 points
- **GIVEN** an artist with has_image=false submitted by user "submitter1"
- **WHEN** an admin approves the artist
- **THEN** a PointEvent with ActionType=ArtistApproved, Points=9 is created, and the user's artist_approval_points is incremented by 9

#### Scenario: Duplicate artist approval does not award double points
- **GIVEN** an artist approval has already been processed for entity_id=42
- **WHEN** the approval is processed again (e.g., admin double-click)
- **THEN** the unique constraint prevents duplicate insertion and points are not incremented again

### Requirement: The admin project SHALL award lyric approval points when approving a lyric
When an admin approves a lyric, the admin project SHALL: look up the lyric's user_id, find or create a users record, INSERT into point_events with ActionType=LyricApproved, Points=15, entity_name=lyric.title (skip if duplicate), increment users.lyric_approval_points by 15, and send an approval notification email.

#### Scenario: Lyric approved earns submitter 15 points
- **GIVEN** a lyric "Song Title" submitted by user "submitter2"
- **WHEN** an admin approves the lyric
- **THEN** a PointEvent with ActionType=LyricApproved, Points=15 is created, and the user's lyric_approval_points is incremented by 15

### Requirement: The admin project SHALL award report acknowledgement points when acknowledging a report
When an admin sets a report status to Acknowledged, the admin project SHALL: look up the report's user_id, find or create a users record, INSERT into point_events with ActionType=ReportAcknowledged, Points=4, entity_name=lyric.title from the joined lyric (skip if duplicate), increment users.report_acknowledgement_points by 4, and send an acknowledgement notification email. Dismissing a report SHALL NOT award points.

#### Scenario: Report acknowledged earns reporter 4 points
- **GIVEN** a lyric report for "Song Title" submitted by user "reporter1"
- **WHEN** an admin acknowledges the report
- **THEN** a PointEvent with ActionType=ReportAcknowledged, Points=4 is created, and the user's report_acknowledgement_points is incremented by 4

#### Scenario: Report dismissed does not earn points
- **GIVEN** a lyric report submitted by user "reporter1"
- **WHEN** an admin dismisses the report
- **THEN** no PointEvent is created and no points are awarded

### Requirement: The admin project SHALL send approval notification emails with required data
When awarding approval points, the admin project SHALL send an email to the original submitter. The email data contract requires: submitter cognito_user_id, entity type ("artist", "lyric", or "report"), entity name, and points awarded. The email content, design, and delivery mechanism are the admin project's responsibility.

#### Scenario: Approval email sent with correct data
- **GIVEN** an artist "Band Name" submitted by user "submitter1" is approved for 10 points
- **WHEN** the approval is processed
- **THEN** an email is triggered with entity_type="artist", entity_name="Band Name", points=10, addressed to the submitter

### Requirement: The admin project SHALL find or create user records for submitters
Before awarding points, the admin project SHALL ensure a users record exists for the submitter. If no record exists, it SHALL create one by resolving the username from Cognito. This mirrors the MVC app's EnsureUserExistsAsync behavior.

#### Scenario: User record created on first approval if not existing
- **GIVEN** no user record exists for the submitter's cognito_user_id
- **WHEN** an admin approves their submission
- **THEN** a user record is created with the resolved username and zero-initialized point columns before the approval points are awarded
