## MODIFIED Requirements

### Requirement: The own profile page SHALL display a per-category points breakdown
When a logged-in user views `/profile`, the page SHALL display a breakdown of their BB points by category: artist submission points, artist approval points, lyric submission points, lyric approval points, report submission points, report acknowledgement points, like-based points, and total BB points. The existing "Want to help?" content SHALL be retained.

#### Scenario: Own profile shows full breakdown
- **GIVEN** a logged-in user with ArtistSubmissionPoints=6, ArtistApprovalPoints=19, LyricSubmissionPoints=25, LyricApprovalPoints=45, ReportSubmissionPoints=3, ReportAcknowledgementPoints=8, and 35 likes given
- **WHEN** they visit `/profile`
- **THEN** the page shows each category total, LikePoints=3, and TotalPoints=109, with ContributorLabel="Contributor"

#### Scenario: Own profile retains existing content
- **GIVEN** a logged-in user visits `/profile`
- **WHEN** the page renders
- **THEN** the existing "Want to help?" content is still visible alongside the new points sections

### Requirement: The own profile page SHALL display a recent activity feed
The own profile page SHALL include a "Recent Activity" section showing the 20 most recent PointEvents in reverse chronological order. Each entry SHALL show: the action description, the entity name, the points earned, and the date. Like-based points are not shown in the activity feed (they are state-based and reflected in the breakdown only).

#### Scenario: Activity feed shows recent events
- **GIVEN** a user with 5 PointEvents
- **WHEN** they visit `/profile`
- **THEN** the activity feed shows all 5 events in reverse chronological order with action, entity name, points, and date

#### Scenario: Activity feed limited to 20 entries
- **GIVEN** a user with 30 PointEvents
- **WHEN** they visit `/profile`
- **THEN** the activity feed shows only the 20 most recent events

#### Scenario: Activity feed empty state for new user
- **GIVEN** a user with 0 PointEvents
- **WHEN** they visit `/profile`
- **THEN** the activity feed shows a friendly message: "You haven't earned any BB Points yet. Submit an artist or lyric to get started!"

#### Scenario: Activity feed empty state for retroactive-only user
- **GIVEN** a user with retroactive points (category totals > 0) but 0 PointEvents
- **WHEN** they visit `/profile`
- **THEN** the activity feed shows: "Your historical contributions have been counted! New activity will appear here as you earn more points."

### Requirement: Visiting the own profile page SHALL dismiss the points-changed indicator
When `GetOwnProfileDataAsync` is called, it SHALL update `User.LastSeenPoints` to the current `TotalPoints`. This ensures the nav bar indicator is dismissed on subsequent page loads.

#### Scenario: Indicator dismissed after profile visit
- **GIVEN** a user with TotalPoints=100 and LastSeenPoints=60
- **WHEN** they visit `/profile`
- **THEN** LastSeenPoints is updated to 100, and the nav bar indicator is not shown on subsequent pages

### Requirement: A logged-in user visiting their own public profile URL SHALL see the own profile view
When a logged-in user navigates to `/profile/{their-own-username}`, the system SHALL redirect them to `/profile` so they see the full breakdown and activity feed, not the restricted public view.

#### Scenario: User redirected from own public URL to own profile
- **GIVEN** a logged-in user with username "myuser"
- **WHEN** they navigate to `/profile/myuser`
- **THEN** they are redirected to `/profile` and see the full own profile view
