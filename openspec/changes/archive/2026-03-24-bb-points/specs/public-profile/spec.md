## ADDED Requirements

### Requirement: The system SHALL provide a public profile page at /profile/{username}
A public profile page SHALL be accessible at `/profile/{username}` for any user (authenticated or anonymous). The page SHALL display: the user's username, total BB points, contributor label, and contribution counts (artists submitted, lyrics submitted). Contribution counts SHALL include all non-deleted submissions (including pending/unapproved) to acknowledge effort. The page SHALL NOT show per-category breakdown or recent activity.

#### Scenario: View public profile of another user
- **GIVEN** a user "contributor1" with TotalPoints=250, 8 artists submitted, 15 lyrics submitted
- **WHEN** any visitor navigates to `/profile/contributor1`
- **THEN** the page displays username "contributor1", TotalPoints=250, ContributorLabel="Regular Contributor", ArtistsSubmitted=8, LyricsSubmitted=15

#### Scenario: Public profile does not show breakdown or activity
- **GIVEN** a user "contributor1" with detailed category points and PointEvents
- **WHEN** a visitor views `/profile/contributor1`
- **THEN** only total points, contributor label, and contribution counts are shown (no category breakdown, no activity feed)

#### Scenario: Anonymous user can view public profiles
- **GIVEN** an unauthenticated visitor
- **WHEN** they navigate to `/profile/contributor1`
- **THEN** the page renders successfully without requiring authentication

#### Scenario: Non-existent username returns 404
- **GIVEN** no user record exists with Username="nonexistent"
- **WHEN** a visitor navigates to `/profile/nonexistent`
- **THEN** a 404 response is returned

#### Scenario: Contribution counts include pending submissions
- **GIVEN** a user with 5 artists (3 approved, 2 pending) and 10 lyrics (7 approved, 3 pending)
- **WHEN** their public profile is viewed
- **THEN** ArtistsSubmitted=5, LyricsSubmitted=10

### Requirement: The /profile/{username} route SHALL NOT conflict with /profile
The routing for `/profile/{username}` and `/profile` (own profile) SHALL be configured so they do not conflict. `/profile` (no parameter) renders the own profile for authenticated users. `/profile/{username}` renders the public profile.

#### Scenario: Routes are distinct and non-conflicting
- **GIVEN** both `/profile` and `/profile/{username}` routes exist
- **WHEN** a logged-in user navigates to `/profile`
- **THEN** the own profile page is rendered (not treated as username parameter)
