## ADDED Requirements

### Requirement: Anonymous protected-action CTAs redirect to login with returnUrl
When an anonymous user clicks a CTA whose intended destination is a protected contribution flow (for example: add artist, add lyric, or report issue), they SHALL be redirected to `/login` with a `returnUrl` query parameter encoding the intended destination URL and source attribution params (`entry_point`, `contribution_type`, `source_section`).

#### Scenario: Anonymous hero CTA redirects to login with encoded returnUrl
- **GIVEN** an anonymous user clicks "Join to add an artist" on the homepage hero
- **WHEN** the browser navigates
- **THEN** the URL SHALL be `/login?returnUrl=%2Fartists%2Fnew%2Fselector%3Fentry_point%3Dhero_add_artist%26contribution_type%3Dartist%26source_section%3Dhero`

#### Scenario: Anonymous artist page CTA redirects to login with encoded returnUrl
- **GIVEN** an anonymous user clicks the "Add one" CTA on artist page for slug "sivan-perwer"
- **WHEN** the browser navigates
- **THEN** the URL SHALL be `/login?returnUrl=%2Fartists%2Fsivan-perwer%2Flyrics%2Fnew%3Fentry_point%3Dartist_header_add_lyric%26contribution_type%3Dlyric`

### Requirement: Anonymous account-creation CTAs may link directly to signup
When an anonymous user clicks a CTA whose primary purpose is account creation rather than immediate entry into a protected contribution form, the CTA MAY link directly to `/signup` instead of `/login`. In that case, source attribution for analytics SHALL be preserved through the signup flow using query parameters, session, TempData, or hidden form fields.

#### Scenario: Homepage create-account CTA links directly to signup
- **GIVEN** an anonymous user clicks the "Create an account" task card on the homepage
- **WHEN** the browser navigates
- **THEN** the URL SHALL be `/signup` with any required source attribution values attached in a way the signup flow can preserve

### Requirement: Post-authentication redirect to intended destination
After successful authentication, the user SHALL be redirected back to the intended destination encoded in the `returnUrl` parameter, including all source attribution query params.

#### Scenario: User lands at intended destination after login
- **GIVEN** a user logs in with `returnUrl` set to `/artists/new/selector?entry_point=hero_add_artist&contribution_type=artist`
- **WHEN** login succeeds and `Url.IsLocalUrl()` validates the URL
- **THEN** the user SHALL be redirected to `/artists/new/selector?entry_point=hero_add_artist&contribution_type=artist`

#### Scenario: returnUrl lost during signup flow falls back to homepage
- **GIVEN** a user goes through signup → confirm → login and returnUrl is not preserved
- **WHEN** login succeeds without a valid returnUrl
- **THEN** the user SHALL land on `/` (homepage) — functionality is preserved but analytics attribution is lost

### Requirement: No modal used for auth redirect
The auth redirect flow SHALL use standard page redirect in v1. No modal SHALL be used.

#### Scenario: CTA click triggers page navigation not modal
- **GIVEN** an anonymous user clicks a contribution CTA
- **WHEN** the click handler executes
- **THEN** a full page navigation to `/login?returnUrl=...` SHALL occur with no modal overlay

### Requirement: returnUrl validation fallback via TempData
If `Url.IsLocalUrl()` rejects the encoded returnUrl (URL with query params), the system SHALL implement a TempData-based fallback: before redirecting to `/login`, store the full destination URL in TempData. After login, check TempData first, then fall back to `returnUrl`.

#### Scenario: TempData fallback used when returnUrl validation fails
- **GIVEN** `Url.IsLocalUrl()` rejects the encoded returnUrl
- **WHEN** the user logs in
- **THEN** the system SHALL check TempData for the stored destination URL and redirect there

### Requirement: returnUrl forwarded through signup link
The login page's link to the signup page SHALL include the current `returnUrl` as a query parameter. The signup flow SHALL preserve `returnUrl` through the confirmation step (via session or hidden form field).

#### Scenario: returnUrl preserved from login to signup page
- **GIVEN** a user on the login page with returnUrl in the URL
- **WHEN** the user clicks the sign-up link
- **THEN** the sign-up page URL SHALL include the same returnUrl parameter

### Requirement: Signup flow explicitly preserves returnUrl and source attribution
If the signup flow is entered with `returnUrl` and/or source attribution values, the GET `/signup`, POST `/signup`, GET `/signup/confirm`, POST `/signup/confirm`, and redirect back to `/login` SHALL preserve those values until the user can authenticate and continue to the intended destination.

#### Scenario: Signup confirmation returns user to login with original returnUrl
- **GIVEN** a user entered `/signup` with a `returnUrl` pointing to `/artists/new/selector?entry_point=hero_add_artist&contribution_type=artist`
- **WHEN** the user completes email confirmation successfully
- **THEN** the redirect to `/login` SHALL include that same `returnUrl`
