## MODIFIED Requirements

### Requirement: Navigation shows conditional auth links based on authentication state
The `_Layout.cshtml` navigation SHALL display "Login" (linking to `/login`) and "Sign up" (linking to `/signup`) when the user is not authenticated, and "Logout" (linking to `/logout`) when the user is authenticated. The "Profile" nav item SHALL only be visible to authenticated users. The "Add" nav item SHALL remain visible to all users (unchanged). The BB Points nav component is unchanged (already handles anonymous users).

#### Scenario: Anonymous user sees login and sign-up links
- **GIVEN** the user is not authenticated
- **WHEN** the navigation renders
- **THEN** "Login" and "Sign up" links are visible, and "Profile" and "Logout" are not visible

#### Scenario: Authenticated user sees logout link
- **GIVEN** the user is authenticated
- **WHEN** the navigation renders
- **THEN** "Logout" link is visible, "Profile" link is visible, and "Login" and "Sign up" are not visible

#### Scenario: Add link visible to all users
- **GIVEN** any user (authenticated or not)
- **WHEN** the navigation renders
- **THEN** the "Add" link is visible

#### Scenario: Anonymous user clicks Add
- **GIVEN** an unauthenticated user
- **WHEN** the user clicks the "Add" link
- **THEN** the cookie challenge redirects to `/login?ReturnUrl={addUrl}`
