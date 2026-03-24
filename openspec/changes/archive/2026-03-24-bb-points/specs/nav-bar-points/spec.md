## ADDED Requirements

### Requirement: The system SHALL render a BbPointsNavViewComponent in the navigation bar for authenticated users
A `BbPointsNavViewComponent` SHALL be created and invoked from `_Layout.cshtml`. For authenticated users, it reads the `sub` claim, calls `IBbPointsService.GetNavBarDataAsync`, and renders a `<li>` element with: the diamond SVG icon (matching existing nav icon sizing, e.g., `class="size-5 lg:size-8 text-neutral-800"`), the total points number, and a link to `/profile`. For unauthenticated users, it returns empty content (nothing rendered).

#### Scenario: Authenticated user sees points in nav bar
- **GIVEN** a logged-in user with TotalPoints=42
- **WHEN** any page is rendered
- **THEN** the navigation bar contains a `<li>` element with the diamond SVG icon and the number "42", linked to `/profile`

#### Scenario: Anonymous user sees no points display
- **GIVEN** an unauthenticated visitor
- **WHEN** any page is rendered
- **THEN** the navigation bar does not contain any BB Points display

#### Scenario: Nav bar points reflect current total including likes
- **GIVEN** a user with 10 category points and 20 likes given
- **WHEN** the page renders
- **THEN** the nav bar shows 12 total points (10 + floor(20/10)=2)

### Requirement: The system SHALL display a visual indicator when points have changed since last session
The diamond SVG icon in the nav bar SHALL have a visual indicator (e.g., subtle glow, dot overlay) when `HasPointsChanged` is true (TotalPoints > LastSeenPoints). When there is no change, the icon renders normally without the indicator.

#### Scenario: Indicator shows when points increased
- **GIVEN** a user with TotalPoints=75 and LastSeenPoints=50
- **WHEN** the nav bar renders
- **THEN** the diamond icon displays with the points-changed visual indicator (glow/dot)

#### Scenario: Indicator is hidden when points match last seen
- **GIVEN** a user with TotalPoints=75 and LastSeenPoints=75
- **WHEN** the nav bar renders
- **THEN** the diamond icon renders normally without any indicator

#### Scenario: Indicator is dismissed after visiting profile
- **GIVEN** a user with TotalPoints=75 and LastSeenPoints=50 (indicator showing)
- **WHEN** the user visits `/profile` (which updates LastSeenPoints to 75)
- **THEN** on subsequent page loads, the indicator is no longer shown

#### Scenario: Clicking the indicator navigates to profile and dismisses it
- **GIVEN** a user with TotalPoints=75 and LastSeenPoints=50 (indicator showing)
- **WHEN** the user clicks the diamond icon in the nav bar
- **THEN** the user is navigated to `/profile` (the entire nav element is a link to /profile), which triggers `GetOwnProfileDataAsync` and updates LastSeenPoints, dismissing the indicator on subsequent pages

### Requirement: The View Component SHALL handle database errors gracefully
If the database query fails during View Component execution, the component SHALL catch the exception, log it at ERROR level, and return empty content. The page renders normally without the points display.

#### Scenario: Database error during nav bar rendering
- **GIVEN** the database is unreachable
- **WHEN** the View Component attempts to render
- **THEN** the exception is caught, logged at ERROR level, and the nav bar renders without any points display (no user-visible error)
