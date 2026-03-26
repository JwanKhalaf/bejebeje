## ADDED Requirements

### Requirement: Analytics constants module with stable event names and enum values
A JavaScript module (e.g., `analytics-constants.js`) SHALL define all event names, and all `entry_point`, `source_section`, `opportunity_type`, and `cta_name` values actually emitted by the v1 implementation, as named constants. No analytics string literals SHALL appear inline in view templates or page scripts.

#### Scenario: Event names are defined as constants
- **GIVEN** the analytics constants module is loaded
- **WHEN** any GA4 event is fired
- **THEN** the event name SHALL be referenced via a constant (e.g., `EVENTS.HOMEPAGE_CTA_CLICKED`), not a string literal

#### Scenario: Entry point values are defined as constants
- **GIVEN** the analytics constants module is loaded
- **WHEN** an entry_point value is used
- **THEN** the value SHALL be referenced via a constant from the module (e.g., `ENTRY_POINTS.HERO_ADD_ARTIST`)

### Requirement: gtag existence check before event firing
All analytics event code SHALL check that `gtag` is defined before calling it, to prevent console errors when gtag.js fails to load (ad blocker, network issue).

#### Scenario: No console errors when gtag is not loaded
- **GIVEN** gtag.js fails to load (e.g., ad blocker)
- **WHEN** an analytics event would fire
- **THEN** the event SHALL silently fail with no console error

### Requirement: Beacon transport for pre-navigation events
Events firing immediately before page navigation (`homepage_cta_clicked`, `auth_redirect_initiated`, `homepage_opportunity_clicked`, `artist_page_cta_clicked`, `lyric_page_cta_clicked`) SHALL use `navigator.sendBeacon` transport (via gtag's `transport_type: 'beacon'` or equivalent) to ensure delivery.

#### Scenario: CTA click event uses beacon transport
- **GIVEN** a user clicks a homepage CTA that triggers navigation
- **WHEN** `homepage_cta_clicked` fires
- **THEN** the event SHALL use beacon transport to ensure delivery before page unload

### Requirement: homepage_search_submitted event
The system SHALL fire `homepage_search_submitted` when a user submits a search from the homepage search box. Parameters SHALL include: source_page ("home"), source_section ("hero_search"), is_authenticated (boolean), ui_variant ("homepage_v1"), query_length (integer). Raw search query text SHALL NOT be sent.

#### Scenario: Search submission fires event with query length
- **GIVEN** a user types "sivan perwer" into the homepage search
- **WHEN** the search is submitted
- **THEN** `homepage_search_submitted` SHALL fire with query_length=12 and source_section="hero_search"

#### Scenario: Search event does not contain raw query text
- **GIVEN** a user submits a homepage search
- **WHEN** the event fires
- **THEN** no parameter SHALL contain the raw search query text

### Requirement: homepage_cta_clicked event
The system SHALL fire `homepage_cta_clicked` when a user clicks any contribution CTA on the homepage. Parameters SHALL include: source_page, source_section, is_authenticated, ui_variant, cta_name, entry_point, destination_type, auth_required.

For v1, `destination_type` SHALL describe the immediate client-side navigation target of the clicked CTA, not the eventual post-auth destination. At minimum, the implementation SHALL use stable constants covering:
- `add_artist_page` for direct links to the artist submission flow
- `login_page` for anonymous protected-action CTAs that first route through `/login`
- `signup_page` for direct account-creation CTAs that route to `/signup`
- `artist_index_page` for indirect browse/discovery links to `/artists`

#### Scenario: Anonymous user clicking hero CTA fires event
- **GIVEN** an anonymous user clicks "Join to add an artist" in the hero
- **WHEN** the CTA is clicked
- **THEN** `homepage_cta_clicked` SHALL fire with source_section="hero", cta_name="add_artist", entry_point="hero_add_artist", auth_required=true

#### Scenario: Authenticated user clicking help grow CTA fires event
- **GIVEN** an authenticated user clicks "Add lyrics" in the "Help grow the archive" section
- **WHEN** the CTA is clicked
- **THEN** `homepage_cta_clicked` SHALL fire with source_section="help_grow_archive", cta_name="add_lyric", entry_point="help_grow_add_lyric", destination_type="artist_index_page", auth_required=false

### Requirement: homepage_opportunity_clicked event
The system SHALL fire `homepage_opportunity_clicked` when a user clicks an item in the "Where help is needed" section. Parameters SHALL include: source_page ("home"), source_section ("where_help_is_needed"), is_authenticated, ui_variant, opportunity_type, artist_id, entry_point ("opportunity_artist_card"), destination_type ("artist_page").

#### Scenario: User clicks opportunity card
- **GIVEN** a user clicks an opportunity card for artist ID 42 with opportunity_type "artists_with_few_lyrics"
- **WHEN** the click occurs
- **THEN** `homepage_opportunity_clicked` SHALL fire with artist_id=42, opportunity_type="artists_with_few_lyrics", entry_point="opportunity_artist_card"

### Requirement: auth_redirect_initiated event
The system SHALL fire `auth_redirect_initiated` when an anonymous user is redirected to login/signup from a contribution CTA. Parameters SHALL include: source_page, source_section, is_authenticated (false), ui_variant, entry_point, contribution_type, redirect_target, post_auth_destination, auth_required (true). This event SHALL fire on homepage, artist detail, and lyric detail pages.

#### Scenario: Auth redirect fires from homepage CTA
- **GIVEN** an anonymous user clicks a homepage contribution CTA
- **WHEN** the redirect to login occurs
- **THEN** `auth_redirect_initiated` SHALL fire with redirect_target="login" and the post_auth_destination matching the intended form URL

#### Scenario: Auth redirect fires from artist page CTA
- **GIVEN** an anonymous user clicks the "Add one" CTA on an artist detail page
- **WHEN** the redirect to login occurs
- **THEN** `auth_redirect_initiated` SHALL fire with source_page="artist_detail" and source_section="artist_header"

### Requirement: contribution_flow_started event
The system SHALL fire `contribution_flow_started` when a user lands on a submission form with source attribution params in the query string. Parameters SHALL include: source_page, source_section (optional), is_authenticated (true), contribution_type, entry_point, flow_step. If attribution params are absent (direct navigation), the event SHALL NOT fire.

#### Scenario: Flow started fires on form page with attribution
- **GIVEN** a user lands on `/artists/new/selector?entry_point=hero_add_artist&contribution_type=artist`
- **WHEN** the page loads
- **THEN** `contribution_flow_started` SHALL fire with flow_step="artist_selector", entry_point="hero_add_artist"

#### Scenario: Flow started does not fire on direct navigation
- **GIVEN** a user navigates directly to `/artists/new/selector` without attribution params
- **WHEN** the page loads
- **THEN** `contribution_flow_started` SHALL NOT fire

### Requirement: contribution_submitted event
The system SHALL fire `contribution_submitted` when a user completes a submission and lands on the post-submission page. Attribution SHALL be preserved via hidden form fields and TempData through the redirect chain. Parameters SHALL include: source_page, source_section (optional), is_authenticated (true), contribution_type, entry_point, submission_status ("submitted").

#### Scenario: Contribution submitted fires after artist creation
- **GIVEN** a user submits an artist form with attribution params
- **WHEN** the post-submission redirect lands on the artist detail page
- **THEN** `contribution_submitted` SHALL fire with contribution_type="artist", submission_status="submitted", and the original entry_point

#### Scenario: Attribution preserved through lyric double-redirect
- **GIVEN** a user submits a lyric form with attribution params
- **WHEN** the redirect goes through `/lyrics/like/{lyricId}` to the lyric detail page
- **THEN** `contribution_submitted` SHALL fire on the final page with the original attribution params from TempData

### Requirement: sign_up_completed event (best effort)
The system SHALL fire `sign_up_completed` on the first app-controlled page after sign-up and login, when the session indicates a first-time login (e.g., OnSigningIn handler sets a flag when a new User record is created). If source attribution is unavailable, the event SHALL be skipped.

#### Scenario: Sign up completed fires on first post-auth page
- **GIVEN** a new user completes sign-up and logs in with returnUrl source params available
- **WHEN** the user lands on the first app-controlled page
- **THEN** `sign_up_completed` SHALL fire with available source attribution

#### Scenario: Sign up completed skipped when attribution unavailable
- **GIVEN** a new user completes sign-up but source params were lost during the flow
- **WHEN** the user lands on the first app-controlled page
- **THEN** `sign_up_completed` SHALL NOT fire

### Requirement: artist_page_cta_clicked event
The system SHALL fire `artist_page_cta_clicked` when a user clicks the contribution CTA on the artist detail page. Parameters SHALL include: source_page ("artist_detail"), source_section ("artist_header"), is_authenticated, cta_name ("add_lyric"), entry_point ("artist_header_add_lyric"), artist_id, auth_required.

#### Scenario: Artist page CTA click fires event
- **GIVEN** an authenticated user clicks "Add one" on artist page for artist ID 42
- **WHEN** the click occurs
- **THEN** `artist_page_cta_clicked` SHALL fire with artist_id=42, cta_name="add_lyric", auth_required=false

### Requirement: lyric_page_cta_clicked event
The system SHALL fire `lyric_page_cta_clicked` when a user clicks the report CTA on the lyric detail page. Parameters SHALL include: source_page ("lyric_detail"), source_section, is_authenticated, cta_name ("report_issue"), entry_point ("lyric_side_report_issue"), lyric_id, auth_required.

#### Scenario: Lyric page report CTA click fires event
- **GIVEN** a user clicks "Report it" on lyric detail page for lyric ID 78
- **WHEN** the click occurs
- **THEN** `lyric_page_cta_clicked` SHALL fire with lyric_id=78, cta_name="report_issue", entry_point="lyric_side_report_issue"

### Requirement: GA4 events must not block page rendering
GA4 events SHALL fire on user interaction only and SHALL NOT block page rendering or degrade user experience.

#### Scenario: Page renders without waiting for event delivery
- **GIVEN** a user interacts with a CTA
- **WHEN** the GA4 event fires
- **THEN** the page navigation or rendering SHALL NOT be delayed by event delivery

### Requirement: No PII in GA4 events
GA4 events SHALL NOT transmit raw search queries, user-entered text, or PII. Only structured, controlled parameter values SHALL be sent.

#### Scenario: No user-entered text in event payloads
- **GIVEN** any GA4 event fires
- **WHEN** the event payload is inspected
- **THEN** no parameter SHALL contain raw user text, only controlled enum values and numeric identifiers

### Requirement: Attribution params in hidden form fields for submission flows
Contribution form pages that load with source attribution params SHALL include these values as hidden form fields. After form submission, the controller SHALL store attribution params in TempData before redirecting, so `contribution_submitted` can fire on the post-submission page.

#### Scenario: Hidden fields preserve attribution through form submission
- **GIVEN** a user loads the lyric form with entry_point and contribution_type query params
- **WHEN** the form renders
- **THEN** hidden fields SHALL contain entry_point, contribution_type, and source_section values

#### Scenario: Controller stores attribution in TempData after submission
- **GIVEN** a user submits a form with attribution hidden fields
- **WHEN** the controller processes the submission and redirects
- **THEN** attribution params SHALL be stored in TempData before the redirect
