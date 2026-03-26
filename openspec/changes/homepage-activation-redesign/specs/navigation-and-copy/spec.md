## ADDED Requirements

### Requirement: Sidebar sign-up label changed to "Join as a contributor"
For unauthenticated users, the sidebar navigation label SHALL change from "Sign up" to "Join as a contributor". The link target SHALL remain `/signup`. The icon SHALL remain unchanged. This SHALL apply to both desktop sidebar and mobile bottom navigation (same markup, different layout via Tailwind responsive classes).

#### Scenario: Anonymous user sees updated sidebar label
- **GIVEN** an unauthenticated user views any page
- **WHEN** the sidebar/bottom navigation renders
- **THEN** the sign-up link text SHALL read "Join as a contributor" pointing to `/signup`

#### Scenario: Authenticated user does not see sign-up link
- **GIVEN** an authenticated user views any page
- **WHEN** the sidebar/bottom navigation renders
- **THEN** no sign-up link SHALL be visible (existing behavior unchanged)

### Requirement: Login page displays contribution-framing message
The login page (`/login`) SHALL always display a short contribution-framing message: "Sign in to contribute lyrics, add artists, and get credit for approved contributions." This message SHALL be displayed unconditionally (not conditional on referrer), placed near the form or alongside existing benefit messaging.

#### Scenario: Login page shows contribution message
- **GIVEN** a user navigates to `/login`
- **WHEN** the page renders
- **THEN** the text "Sign in to contribute lyrics, add artists, and get credit for approved contributions" SHALL be visible

### Requirement: Signup page displays contribution-framing message
The signup page (`/signup`) SHALL display the same contribution-framing message as the login page, always displayed. This page uses `_AuthLayout` which does NOT load gtag.js — no GA4 events fire on this page.

#### Scenario: Signup page shows contribution message
- **GIVEN** a user navigates to `/signup`
- **WHEN** the page renders
- **THEN** a contribution-framing message SHALL be visible strengthening the credit and BB Points angle
