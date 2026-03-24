## ADDED Requirements

### Requirement: The system SHALL award points after artist creation based on photo inclusion
After the artist creation flow completes (including image upload if applicable), the `ArtistController` SHALL determine points (5 if HasImage=true, 1 if HasImage=false), call `AwardSubmissionPointsAsync` with ActionType=ArtistSubmitted, and set TempData with the result. Points are awarded AFTER the image upload step completes, not after the initial artist record creation. This applies to both individual artist and band artist creation paths.

#### Scenario: Artist created with photo earns 5 points
- **GIVEN** a logged-in user within the daily artist submission limit
- **WHEN** they submit a new artist with a photo that uploads successfully
- **THEN** a PointEvent is created with ActionType=ArtistSubmitted, Points=5, and TempData indicates points were earned

#### Scenario: Artist created without photo earns 1 point
- **GIVEN** a logged-in user within the daily artist submission limit
- **WHEN** they submit a new artist without a photo
- **THEN** a PointEvent is created with ActionType=ArtistSubmitted, Points=1, and TempData indicates points were earned

#### Scenario: Artist submission exceeds daily limit
- **GIVEN** a logged-in user who has already submitted 5 artists today (the default limit)
- **WHEN** they submit another artist
- **THEN** the artist is saved normally, no PointEvent is created, and TempData indicates points were NOT earned

#### Scenario: Band artist creation also awards points
- **GIVEN** a logged-in user within the daily limit
- **WHEN** they submit a new band artist via the CreateBand action
- **THEN** points are awarded using the same logic as individual artist creation

### Requirement: The system SHALL award points after lyric creation
After lyric creation, the `LyricController` SHALL call `AwardSubmissionPointsAsync` with ActionType=LyricSubmitted, Points=5, and set TempData with the result. TempData SHALL survive the existing double redirect (Create -> Like -> Lyric detail page).

#### Scenario: Lyric submission earns 5 points
- **GIVEN** a logged-in user within the daily lyric submission limit
- **WHEN** they submit a new lyric
- **THEN** a PointEvent is created with ActionType=LyricSubmitted, Points=5, and TempData indicates points were earned

#### Scenario: Lyric submission exceeds daily limit
- **GIVEN** a logged-in user who has already submitted 10 lyrics today (the default limit)
- **WHEN** they submit another lyric
- **THEN** the lyric is saved normally, no PointEvent is created, and TempData indicates points were NOT earned

#### Scenario: TempData survives the double redirect through Like action
- **GIVEN** a user submits a lyric and is redirected through Create -> Like -> Lyric detail
- **WHEN** the lyric detail page renders
- **THEN** the TempData notification banner is displayed (TempData persists until read)

### Requirement: The system SHALL award points after report submission and remove the blocking rate limit
The `ReportController` SHALL remove the existing daily limit check that redirects to the LimitReached page. The report form SHALL always be shown (duplicate check remains). After saving the report, the controller SHALL call `AwardSubmissionPointsAsync` with ActionType=ReportSubmitted, Points=1, and set TempData. The LimitReached route and view SHALL be removed.

#### Scenario: Report submission earns 1 point
- **GIVEN** a logged-in user within the daily report submission limit
- **WHEN** they submit a lyric report
- **THEN** the report is saved, a PointEvent is created with ActionType=ReportSubmitted, Points=1, and TempData indicates points were earned

#### Scenario: Report submission exceeds daily limit
- **GIVEN** a logged-in user who has already submitted 5 reports today (the default limit)
- **WHEN** they submit another report
- **THEN** the report is saved normally, no PointEvent is created, and TempData indicates points were NOT earned

#### Scenario: Report form is always accessible regardless of daily limit
- **GIVEN** a logged-in user who has submitted 5 reports today
- **WHEN** they navigate to the report form
- **THEN** the form is displayed (no redirect to LimitReached)

#### Scenario: Duplicate pending report check remains
- **GIVEN** a user already has a pending report for the same lyric
- **WHEN** they attempt to submit another report
- **THEN** the duplicate check prevents submission (existing behavior preserved)

#### Scenario: LimitReached route is removed
- **GIVEN** the application is running with BB Points enabled
- **WHEN** a request is made to the old LimitReached route
- **THEN** a 404 response is returned

### Requirement: Approval-based points SHALL always be awarded regardless of daily limits
When an admin approves an artist, approves a lyric, or acknowledges a report, approval points SHALL be awarded to the original submitter regardless of whether the original submission exceeded the daily limit. Approvals are not subject to daily limits.

#### Scenario: Approval points awarded for a submission that exceeded daily limit
- **GIVEN** a user submitted an artist that did not earn submission points (daily limit exceeded)
- **WHEN** an admin approves that artist
- **THEN** the user earns full approval points (9 or 10 depending on photo)
