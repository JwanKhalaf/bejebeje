# SPEC.md — Homepage Activation Redesign

## 1. Overview

### Purpose

Redesign the Bejebeje homepage from a passive browse surface into an activation surface that converts cultural interest into first contributions.

### Problem

The current homepage leads with activity-based modules (recent submissions, recently verified, popular female artists) that signal inactivity on a quiet platform. It has no contribution prompts, no participation thesis, and no bridge from reading to acting.

### Solution summary

Replace the existing homepage modules with a contribution-led layout that works regardless of platform velocity. The new homepage prioritises search, frames contribution as culturally meaningful, surfaces concrete contribution opportunities from existing data, and guides anonymous visitors toward sign-up and first contribution. Small CTA additions are also made to artist and lyric detail pages to complete the contribution funnel beyond the homepage.

---

## 2. Goals & Non-Goals

### Goals

- Convert the homepage into an activation surface that drives sign-ups and first contributions.
- Replace stale activity-led modules with contribution-led modules that work at any platform size.
- Make contribution feel visible, meaningful, and culturally significant.
- Give anonymous visitors a clear next step toward participation.
- Preserve search as the primary discovery mechanism.
- Instrument GA4 events to measure activation funnel performance.
- Add lightweight contribution CTAs to artist and lyric detail pages.

### Non-goals

- A/B testing infrastructure — compare before/after via GA4.
- Homepage personalisation (welcome back, pending submissions) — phase 2.
- Featured contributors or ranking systems.
- Author page changes.
- Public surfacing of reported lyrics.
- New taxonomy fields (region, dialect, era).
- Admin UI for curated homepage sections.
- Cognito hosted sign-up form redesign.
- Custom analytics infrastructure beyond GA4.
- "Artists missing a photo" module.
- Full-site engagement redesign.

---

## 3. Assumptions & Constraints

### Constraints

1. **No new database tables or fields.** All data is derived from existing schema: `artists`, `lyrics`, `users`, `point_events`, `artist_slugs`, `lyric_slugs`.
2. **No new admin tooling.** Curated sections and opportunity queries are data-driven, not admin-UI-driven.
3. **No Cognito hosted form changes.** The Cognito hosted authentication UI (if used for any flows) is not modified. The application-owned login (`/login`) and signup (`/signup`) pages are MVC views under our control and are in scope for copy changes.
4. **No new standalone pages.** The "how contributions work" content is inline on the homepage.
5. **Existing layout preserved.** Desktop sidebar + mobile bottom navigation remain unchanged structurally.
6. **Tailwind CSS 4.0.3.** All styling uses the existing Tailwind setup.
7. **Single-instance deployment.** The application runs as a single Docker container. Caching strategy assumes single-instance.

### Assumptions

1. GA4 (`gtag.js`) is loaded on all pages served by `_Layout.cshtml`. It is **not** loaded on auth pages (`_AuthLayout.cshtml`). This is confirmed by codebase inspection.
2. The existing login flow supports `returnUrl` as a query parameter and redirects to it after successful authentication. Confirmed: `AccountController.Login` accepts `returnUrl` and `Url.IsLocalUrl()` validates it before redirect.
3. The existing `GetTopTenFemaleArtistsByLyricsCountAsync()` query structure can be reused with minor filter corrections (see section 5).
4. Artist and lyric submission flows, forms, validation, and BB Points awarding remain unchanged. The homepage only links to them.
5. The approval workflow remains manual and unchanged.
6. The threshold of "fewer than 3 approved lyrics" for opportunity cards is appropriate for the current archive size and may be adjusted post-launch.
7. All contribution routes (`/artists/new/*`, `/artists/{slug}/lyrics/new`, `/artists/{slug}/lyrics/{slug}/report`) are already protected by `[Authorize]` attributes.

---

## 4. System Architecture

### High-level description

The change is scoped to the MVC presentation layer and the service layer. No data model changes. No new external integrations. The primary additions are:

1. A new `IHomepageService` that orchestrates homepage-specific queries and caching.
2. A new `HomepageViewModel` replacing the existing `IndexViewModel`.
3. A redesigned homepage view (`Views/Home/Index.cshtml`).
4. Minor view modifications to artist detail, lyric detail, login, signup, and layout pages.
5. Client-side JavaScript for GA4 event instrumentation.
6. An analytics constants module for stable event names and parameter values.

### Component responsibilities

```
┌─────────────────────────────────────────────────────────┐
│                    _Layout.cshtml                        │
│  (sidebar nav label change, gtag.js already loaded)     │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  HomeController.Index()                                 │
│    │                                                    │
│    ├── IHomepageService.GetHomepageViewModelAsync()      │
│    │     │                                              │
│    │     ├── query: opportunity cards (artists w/       │
│    │     │   few lyrics + new artists needing lyrics)   │
│    │     │                                              │
│    │     ├── cached: community impact stats             │
│    │     │   (IMemoryCache, 30-min TTL)                 │
│    │     │                                              │
│    │     └── query: top female artists (existing,       │
│    │         with corrected filters)                    │
│    │                                                    │
│    └── Views/Home/Index.cshtml (redesigned)             │
│          │                                              │
│          └── analytics.js (GA4 event constants +        │
│              event firing via gtag.js)                   │
│                                                         │
├─────────────────────────────────────────────────────────┤
│  Modified pages:                                        │
│  - Views/Lyric/ArtistLyrics.cshtml (add CTA)           │
│  - Views/Lyric/Lyric.cshtml (copy strengthening)       │
│  - Views/Account/Login.cshtml (contribution message)   │
│  - Views/Account/Signup.cshtml (contribution message)  │
└─────────────────────────────────────────────────────────┘
```

### System boundaries

- **In scope for code changes:** HomeController, IHomepageService (new), HomepageViewModel (new), homepage view, artist detail view, lyric detail view, login view, signup view, layout (sidebar), client-side analytics JS, existing `GetTopTenFemaleArtistsByLyricsCountAsync` query (filter correction).
- **Unchanged:** All submission forms, approval workflows, BB Points logic, Cognito integration, API endpoints, email service, image service, sitemap service.

### External dependencies

| Dependency | Type | Change required |
|---|---|---|
| PostgreSQL | Database | No schema changes. New read queries only. |
| AWS Cognito | Authentication | No changes. Existing `returnUrl` mechanism used. |
| Google Analytics 4 | Client-side tracking | New custom events pushed via existing `gtag.js`. |

---

## 5. Data Model & State

### No schema changes

No new tables, columns, or indexes are introduced. All data is derived from existing entities.

### Existing entities used

**Artist** (table: `artists`)
- `id`, `first_name`, `last_name`, `full_name`, `is_approved`, `is_deleted`, `has_image`, `sex`, `created_at`

**Lyric** (table: `lyrics`)
- `id`, `title`, `is_approved`, `is_deleted`, `artist_id`, `created_at`

**ArtistSlug** (table: `artist_slugs`)
- `artist_id`, `name`, `is_primary`

**User** (table: `users`)
- `id`, `cognito_user_id`

**PointEvent** (table: `point_events`)
- `id`, `user_id`, `action_type`, `created_at`

### New queries

**Q1. Artists with few lyrics** (for "Where help is needed")

Return approved, non-deleted artists with fewer than 3 approved, non-deleted lyrics. Order by approved lyric count ascending (least lyrics first), then by `artists.created_at` descending (newest first as tiebreaker). Limit configurable (default 8). Each result includes: `artist_id`, `artist_name` (full_name), `primary_slug`, `has_image`, `approved_lyric_count`. Tag each result with `opportunity_type = "artists_with_few_lyrics"`.

**Q2. Newly added artists needing lyrics** (for "Where help is needed")

Return approved, non-deleted artists where `created_at` is within the last 90 days and approved, non-deleted lyric count is fewer than 3. Order by `created_at` descending. Limit configurable (default 4). Same result shape as Q1. Tag each result with `opportunity_type = "new_artists_needing_lyrics"`.

**Q3. Community impact stats** (for stats block, cached)

Three scalar counts:
- Total approved lyrics: `COUNT(*) FROM lyrics WHERE is_approved = true AND is_deleted = false`
- Total approved artists: `COUNT(*) FROM artists WHERE is_approved = true AND is_deleted = false`
- Total contributors: `COUNT(DISTINCT user_id) FROM point_events WHERE action_type IN (2, 4)` (where 2 = `ArtistApproved`, 4 = `LyricApproved`). This is the v1 implementation. It couples a homepage product metric to the gamification event model, which is a known trade-off accepted because `point_events` is the only existing table that reliably records which users have had content approved, and it avoids a more expensive union query across `artists` and `lyrics`.

**Q4. Top female artists (corrected)** (for curated discovery)

Modify the existing `GetTopTenFemaleArtistsByLyricsCountAsync()` query to add filters: `a.is_approved = true AND a.is_deleted = false`. Also filter lyrics in the count: `l.is_approved = true AND l.is_deleted = false`. Keep existing limit of 10, order by lyric count descending.

### Opportunity card composition

The "Where help is needed" section combines Q1 and Q2 results:

1. Execute Q2 (new artists, limit 4).
2. Execute Q1 (all artists with few lyrics, limit 8), excluding artist IDs already returned by Q2.
3. Concatenate: Q2 results first, then Q1 results. Cap total at 8.

Each card retains its `opportunity_type` tag for analytics.

### Cache state

| Cache key | Value type | TTL | Eviction |
|---|---|---|---|
| `homepage:community_impact_stats` | `CommunityImpactStatsViewModel` | 30 minutes absolute expiration | Time-based only. No manual invalidation in v1. |

On cache miss, stats are computed on-demand from the database and cached immediately.

---

## 6. Interfaces & Integration Points

### New service interface

**`IHomepageService`**

| Method | Returns | Description |
|---|---|---|
| `GetHomepageViewModelAsync(bool isAuthenticated)` | `HomepageViewModel` | Orchestrates all homepage data: opportunity cards, cached stats, curated artists. `isAuthenticated` flag is passed through to the view model for CTA variant rendering. |

### New view models

**`HomepageViewModel`** (replaces `IndexViewModel`)

| Property | Type | Description |
|---|---|---|
| `IsAuthenticated` | `bool` | Whether the current user is logged in. Drives CTA label/href variants. |
| `OpportunityCards` | `IReadOnlyList<OpportunityCardViewModel>` | Combined, deduplicated, capped at 8. |
| `CommunityImpact` | `CommunityImpactStatsViewModel` | Cached aggregate stats. Null only on catastrophic failure (see section 9). |
| `FemaleArtists` | `IEnumerable<RandomFemaleArtistItemViewModel>` | Existing type, reused. Now with corrected query filters. |

**`OpportunityCardViewModel`**

| Property | Type | Description |
|---|---|---|
| `ArtistId` | `int` | Internal artist ID (for analytics). |
| `ArtistName` | `string` | Display name (`full_name`). |
| `ArtistSlug` | `string` | Primary slug for URL construction. |
| `HasImage` | `bool` | Whether the artist has an uploaded image. |
| `ApprovedLyricCount` | `int` | Current count of approved, non-deleted lyrics. |
| `OpportunityType` | `string` | `"artists_with_few_lyrics"` or `"new_artists_needing_lyrics"`. Maps to analytics enum. |

**`CommunityImpactStatsViewModel`**

| Property | Type | Description |
|---|---|---|
| `TotalApprovedLyrics` | `int` | |
| `TotalApprovedArtists` | `int` | |
| `TotalContributors` | `int` | |

### Modified service method

**`IArtistsService.GetTopTenFemaleArtistsByLyricsCountAsync()`**

Add `WHERE a.is_approved = true AND a.is_deleted = false` to the artist filter, and `l.is_approved = true AND l.is_deleted = false` to the lyric count join. No signature change. No new return type.

### Page / view inventory

#### P1. Homepage (redesigned)

- **Route:** `/` (`HomeController.Index`)
- **Purpose:** Primary activation surface combining search, contribution prompts, opportunity discovery, and social proof.
- **Layout:** Main content area within existing sidebar/bottom-nav layout.
- **Sections (in order):**

| # | Section | Description |
|---|---|---|
| 1 | **Hero area** | Headline framing cultural preservation; supporting text; search input (existing functionality); primary CTA ("Add an artist" / "Join to add an artist"); secondary CTA ("Contribute a lyric" / "Join to contribute a lyric"). |
| 2 | **Help grow the archive** | 3–4 task cards (add artist, add lyric, report issue, create account for anonymous). Each card has a one-line description and a link. "Create account" card hidden for authenticated users (FR-7). |
| 3 | **How contributions work** | Compact inline explainer: 4 steps (submit → review → publish + credit → earn BB Points). Not a separate page. **Content guidance:** The explainer must lead with the cultural contribution and recognition steps. BB Points appear as a supporting benefit in the final step, not as the primary motivation. The tone should feel like "here's how you help preserve Kurdish music" rather than "here's how you earn points." |
| 4 | **Where help is needed** | Up to 8 opportunity cards showing artists that need lyrics. Each card shows artist name, links to artist detail page. Invitation framing ("Help complete these artist pages"). Graceful degradation if zero results (see section 9). |
| 5 | **Community impact** | Three stat counters: approved lyrics, approved artists, contributors. Always shown. Framing copy makes small numbers purposeful (e.g. "X lyrics preserved and growing"). |
| 6 | **Curated discovery** | "Women of Kurdish music" — top 10 female artists by lyric count. Editorial framing, not a raw data listing. |

- **Data:** `HomepageViewModel` from `IHomepageService`.
- **CTA behaviour by action type:**

| Action | Authenticated destination | Anonymous destination | CTA type |
|---|---|---|---|
| Add an artist | `/artists/new/selector` (direct) | `/login?returnUrl=<encoded /artists/new/selector + source params>` | Direct entry |
| Contribute a lyric | `/artists` (indirect — user browses artists, then uses artist page CTA to add lyric) | `/login?returnUrl=<encoded /artists + source params>` | Indirect via browse |
| Report an issue | No direct link target (no lyric context on homepage). Card is instructional: explains that reporting is available on any lyric page. Links to `/artists` as a discovery entry point. | Same as authenticated — instructional card linking to `/artists` | Instructional / discovery |
| Create an account | Hidden (FR-7) | `/signup` | Direct entry (anonymous only) |

- **Anonymous CTA copy convention:** Anonymous CTAs use "Join to..." phrasing (e.g., "Join to add an artist", "Join to contribute a lyric") rather than "Sign up to..." to better match the entry surface, which may be a login page rather than a sign-up page. This aligns with the sidebar label change ("Join as a contributor", FR-17) and avoids a mismatch where copy says "Sign up" but the user first lands on a combined login/sign-up entry point. All anonymous CTAs route through `/login?returnUrl=...`; the login page includes a link to the sign-up form for users who need to create an account.

#### P2. Artist detail page (modified)

- **Route:** `/artists/{artistSlug}/lyrics` (`LyricController.ArtistLyrics`)
- **Change:** Add a lightweight contribution CTA: "Know another lyric by this artist? Add one."
- **Link target (authenticated):** `/artists/{artistSlug}/lyrics/new`
- **Link target (anonymous):** `/login?returnUrl=%2Fartists%2F{artistSlug}%2Flyrics%2Fnew%3Fentry_point%3Dartist_header_add_lyric%26contribution_type%3Dlyric`
- **Placement:** Below the artist info / above the lyrics list. Copy and link only — no layout restructuring.
- **Analytics:** `artist_page_cta_clicked` event on click.

#### P3. Lyric detail page (modified)

- **Route:** `/artists/{artistSlug}/lyrics/{lyricSlug}` (`LyricController.Lyric`)
- **Change:** Strengthen existing report link with visible text copy: "Spot an error? Report it." The existing flag icon is retained; the text label is added alongside it.
- **Link target:** Existing report route (`/artists/{artistSlug}/lyrics/{lyricSlug}/report`). Auth redirect behaviour unchanged (protected by `[Authorize]`; framework redirects to `/login` automatically).
- **Analytics:** `lyric_page_cta_clicked` event on click.

#### P4. Login page (modified)

- **Route:** `/login` (`AccountController.Login`)
- **Change:** Add a short contribution-framing message, always displayed (not conditional on referrer): "Sign in to contribute lyrics, add artists, and get credit for approved contributions."
- **Placement:** Within the existing login page layout, near the form or alongside the existing benefit messaging.
- **Note:** The login page already has some benefit messaging ("Add missing artists and lyrics", "Help protect cultural memory"). The new copy reinforces the contribution + credit + BB Points framing. Uses "Sign in to..." rather than "Create an account to..." since this page serves returning users.

#### P5. Signup page (modified)

- **Route:** `/signup` (`AccountController.Signup`)
- **Change:** Same contribution-framing message as login page, always displayed.
- **Ownership:** This is an application-owned MVC page (`Views/Account/Signup.cshtml`), not a Cognito-hosted form. It uses `_AuthLayout` but is fully under our control. The "no Cognito hosted form changes" constraint (section 3) refers to Cognito's own hosted authentication UI, not this page.
- **Note:** The signup page already has related messaging ("Become part of preserving Kurdish lyrics"). The new copy strengthens the credit and BB Points angle. The `_AuthLayout` does not load `gtag.js`, so no GA4 events fire on this page — this is expected and handled by the `sign_up_completed` best-effort approach (section 6, GA4 integration).

#### P6. Sidebar / bottom navigation (modified)

- **File:** `Views/Shared/_Layout.cshtml`
- **Change:** For unauthenticated users, change the "Sign up" label to "Join as a contributor". The link target remains `/signup`. The icon remains unchanged.
- **Responsive behaviour:** Applies to both desktop sidebar and mobile bottom navigation (same markup, different layout via Tailwind responsive classes).

### GA4 integration

GA4 events are fired client-side via `gtag('event', ...)` using the existing `gtag.js` already loaded in `_Layout.cshtml`.

**Analytics constants module:** A JavaScript module (e.g., `analytics-constants.js`) must define all event names, `entry_point` values, `source_section` values, `opportunity_type` values, and `cta_name` values as named constants. No analytics string literals should appear inline in view templates or page scripts.

**Event schema:** The full event schema — including event definitions, parameter tables, naming conventions, entry point enums, example payloads, and implementation rules — is defined in `analytics-and-event-tracking.md`. That document is the authoritative reference. The events required for v1 are:

| Event | Trigger | Pages |
|---|---|---|
| `homepage_search_submitted` | User submits search from homepage | Homepage |
| `homepage_cta_clicked` | User clicks any contribution CTA on homepage | Homepage |
| `homepage_opportunity_clicked` | User clicks an item in "Where help is needed" | Homepage |
| `auth_redirect_initiated` | Anonymous user redirected to login from a CTA | Homepage, artist detail, lyric detail |
| `contribution_flow_started` | User lands on a submission form with source attribution params | Artist form, lyric form, report form |
| `contribution_submitted` | User completes a submission with source attribution params | Artist form, lyric form, report form |
| `sign_up_completed` | User returns to app after sign-up (best effort) | Post-auth landing page |
| `artist_page_cta_clicked` | User clicks contribution CTA on artist detail page | Artist detail |
| `lyric_page_cta_clicked` | User clicks contribution CTA on lyric detail page | Lyric detail |

**Event transport:** Use `navigator.sendBeacon` transport (via gtag's `transport_type: 'beacon'` or equivalent) for events that fire immediately before a page navigation (e.g., `homepage_cta_clicked`, `auth_redirect_initiated`) to ensure delivery.

**`contribution_flow_started` and `contribution_submitted` attribution:** These events fire on pages outside the homepage (artist form, lyric form, report form). Source attribution is carried via query parameters on the destination URL (see section 7, Core Flow 2). The form page JavaScript reads `entry_point`, `contribution_type`, and `source_section` from the query string and includes them in the event payload. If these params are absent (user navigated directly), the events do not fire — this avoids false attribution.

**`contribution_submitted` attribution preservation:** When a contribution form page loads with source attribution params, the form must include these values as hidden fields. After successful form submission, the controller redirects to a post-submission page. The attribution params must be available on that page (via query params appended to the redirect URL, or via TempData set by the controller before redirect) to fire `contribution_submitted`.

The actual post-submission redirect targets in the existing codebase are:

| Submission type | Redirect chain | Final landing page |
|---|---|---|
| Artist (individual or band) | POST `/artists/new/individual` → 302 | `/artists/{artistSlug}/lyrics` (artist detail page) |
| Lyric | POST `/artists/{artistSlug}/lyrics/new` → 302 to `/lyrics/like/{lyricId}` (intermediate, auto-likes the lyric, route: `[Route("lyrics/like/{lyricId}")]`) → 302 | `/artists/{artistSlug}/lyrics/{lyricSlug}` (lyric detail page) |
| Report | POST `/artists/{artistSlug}/lyrics/{lyricSlug}/report` → 302 | `/artists/{artistSlug}/lyrics/{lyricSlug}/report/thank-you` (thank-you page) |

**Note on the lyric redirect chain:** The intermediate `/lyrics/like/{lyricId}` route does not follow the `/artists/...` path convention used elsewhere in the app. It is a real route defined as `[Route("lyrics/like/{lyricId}")]` in `LyricController`. The `artistSlug` and `lyricSlug` values are passed as query parameters to this intermediate route and used to construct the final 302 to the lyric detail page. `contribution_submitted` fires on the **final landing page**, not the intermediate route.

For all three submission types, TempData is the recommended mechanism for carrying attribution params through the redirect chain. All three flows already use TempData on these redirect paths for BB Points confirmation messages, so the pattern is established. Engineers should verify early in implementation that TempData values survive the lyric submission's double-redirect (POST → intermediate → final).

**`sign_up_completed` (best effort):** Since `_AuthLayout` does not load `gtag.js`, this event cannot fire on auth pages. Instead, it fires on the first app-controlled page the user lands on after completing sign-up and login. Implementation: when the user arrives at a page via `returnUrl` after authentication, and the session indicates this is a first-time login (e.g., the `OnSigningIn` handler sets a flag when a new User record is created), fire the event with whatever source attribution is available from the query params. If source attribution is unavailable, skip the event.

---

## 7. Core Flows

### Core flow 1: Anonymous user views homepage

1. User navigates to `/`.
2. `HomeController.Index` calls `IHomepageService.GetHomepageViewModelAsync(isAuthenticated: false)`.
3. Service executes opportunity card queries (Q1 + Q2), retrieves cached community impact stats (or computes on miss), and calls the corrected female artists query (Q4).
4. Service composes `HomepageViewModel` with `IsAuthenticated = false`.
5. View renders with anonymous CTA variants:
   - Hero primary CTA: "Join to add an artist" → `href="/login?returnUrl=%2Fartists%2Fnew%2Fselector%3Fentry_point%3Dhero_add_artist%26contribution_type%3Dartist"`
   - Hero secondary CTA: "Join to contribute a lyric" → `href="/login?returnUrl=%2Fartists%3Fentry_point%3Dhero_add_lyric%26contribution_type%3Dlyric"`
   - "Help grow the archive" includes 4 task cards (add artist, add lyric, report issue, create account).
   - "Where help is needed" shows up to 8 opportunity cards linking to artist detail pages.
   - Community impact stats render with framing copy.
   - Curated discovery section renders female artists.
6. GA4 receives standard `page_view` from gtag config (automatic).

**Day-1 verification:** All data rendered on the homepage comes from existing approved, non-deleted records. Opportunity cards link to artist detail pages which exist for all approved artists. Community impact stats are simple counts that work at any scale including zero (framing copy handles small numbers). The curated discovery section reuses the existing female artists query with corrected filters.

### Core flow 2: Anonymous user clicks homepage CTA → sign-up → contribution

1. Anonymous user clicks "Join to add an artist" on homepage hero.
2. Client-side JS fires `homepage_cta_clicked` with `entry_point: "hero_add_artist"`, `source_section: "hero"`, `auth_required: true`.
3. Client-side JS fires `auth_redirect_initiated` with `entry_point: "hero_add_artist"`, `contribution_type: "artist"`, `redirect_target: "login"`, `post_auth_destination: "/artists/new/selector"`.
4. Browser navigates to `/login?returnUrl=%2Fartists%2Fnew%2Fselector%3Fentry_point%3Dhero_add_artist%26contribution_type%3Dartist%26source_section%3Dhero`.
5. Login page renders with `returnUrl` stored in form. User sees contribution-framing message (FR-23). User sees link to sign-up page.
6. **If user needs to create an account:** User clicks sign-up link. The `returnUrl` must be forwarded to the signup flow. Implementation: the sign-up link on the login page includes `?returnUrl=...` as a query param. The signup flow preserves `returnUrl` through the confirmation step (store in session or hidden form field). After email confirmation, user is directed to login with the original `returnUrl` attached.
7. User logs in successfully. `AccountController.Login` POST validates credentials, signs in, checks `returnUrl` via `Url.IsLocalUrl()`, and redirects to `/artists/new/selector?entry_point=hero_add_artist&contribution_type=artist&source_section=hero`.
8. User lands on artist selector page. Page JS reads `entry_point`, `contribution_type`, `source_section` from query params. Fires `contribution_flow_started` with `flow_step: "artist_selector"`, `entry_point: "hero_add_artist"`, `contribution_type: "artist"`.
9. User completes artist form and submits. Form includes hidden fields for attribution params. Controller processes submission, awards BB Points, stores attribution params in TempData, and redirects to `/artists/{artistSlug}/lyrics` (the new artist's detail page).
10. Artist detail page loads. Page JS reads attribution params from TempData (rendered as data attributes or inline JSON by the view). Fires `contribution_submitted` with `entry_point: "hero_add_artist"`, `contribution_type: "artist"`, `submission_status: "submitted"`.

**Day-1 verification:** The login flow's `returnUrl` handling is already implemented and tested. The `Url.IsLocalUrl()` check accepts local URLs with query strings. The `[Authorize]` attribute on `/artists/new/selector` does not interfere because the user is now authenticated. The existing artist submission form, validation, and BB Points awarding are unchanged. The post-submission redirect to `/artists/{artistSlug}/lyrics` is the existing behaviour — TempData is already used on this redirect path for confirmation messages.

**Fallback:** If `returnUrl` is lost during the signup → confirm → login flow, the user lands on `/` (homepage) after login. Attribution is lost but functionality is preserved. This is acceptable degradation per Risk R5.

### Core flow 3: Authenticated user clicks opportunity card → adds lyric

1. Authenticated user views homepage. CTAs show direct action labels.
2. User clicks an opportunity card for an artist with few lyrics.
3. Client-side JS fires `homepage_opportunity_clicked` with `opportunity_type`, `artist_id`, `artist_slug`, `entry_point: "opportunity_artist_card"`.
4. Browser navigates to `/artists/{artistSlug}/lyrics` (artist detail page).
5. User sees the artist's existing lyrics and the new CTA: "Know another lyric by this artist? Add one."
6. User clicks the CTA. JS fires `artist_page_cta_clicked` with `entry_point: "artist_header_add_lyric"`, `artist_id`, `cta_name: "add_lyric"`.
7. Browser navigates to `/artists/{artistSlug}/lyrics/new?entry_point=artist_header_add_lyric&contribution_type=lyric&source_section=artist_header`.
8. Lyric form page JS reads attribution params and fires `contribution_flow_started` with `flow_step: "lyric_form"`.
9. User fills out and submits the lyric. Attribution preserved via hidden fields. Controller processes submission, awards BB Points, stores attribution params in TempData, and redirects through the like intermediate action to the lyric detail page.
10. Lyric detail page loads. Page JS reads attribution params from TempData (rendered as data attributes or inline JSON by the view). Fires `contribution_submitted` with `entry_point: "artist_header_add_lyric"`, `contribution_type: "lyric"`, `submission_status: "submitted"`.

**Day-1 verification:** The artist detail page renders for all approved artists. The "add lyric" link (`/artists/{artistSlug}/lyrics/new`) is an existing route that works for authenticated users. The lyric form, submission, and BB Points awarding are unchanged. The post-submission redirect goes through `/lyrics/like/{lyricId}` (intermediate) and lands on the lyric detail page — TempData survives this redirect chain (single server, cookie-based TempData). Engineers should verify TempData persistence through this double redirect early in implementation.

### Core flow 4: Homepage with zero opportunity results

1. `IHomepageService` executes Q1 and Q2. Both return zero results (all artists have 3+ lyrics).
2. `HomepageViewModel.OpportunityCards` is an empty list.
3. View detects empty list and renders a positive fallback message: "The archive is in great shape — but there are always more artists to add!" with a link to the add-artist flow.
4. The section heading ("Where help is needed") still renders. The fallback message replaces the card grid.

**Day-1 verification:** The empty-state rendering is handled entirely in the view with a simple conditional. No broken layout or empty container is shown.

### Core flow 5: Community impact stats cache lifecycle

1. First homepage request after application start. Cache is empty.
2. `IHomepageService` checks `IMemoryCache` for `homepage:community_impact_stats`. Cache miss.
3. Service executes the three COUNT queries (Q3) against the database.
4. Service stores the result in `IMemoryCache` with 30-minute absolute expiration.
5. Subsequent homepage requests within 30 minutes hit the cache. No database queries for stats.
6. After 30 minutes, entry expires. Next request triggers a fresh computation and re-caches.

**Day-1 verification:** `IMemoryCache` is a built-in ASP.NET Core service. It requires registration in DI (`builder.Services.AddMemoryCache()`) and injection into `HomepageService`. No external infrastructure needed.

---

## 8. Non-Functional Requirements

### Performance

- **NFR-1.** Community impact stats must be served from `IMemoryCache` (30-minute TTL). On cache miss, compute from database and cache immediately.
- **NFR-2.** Homepage must render in under 2 seconds on a standard connection. The opportunity card queries (Q1, Q2) and female artist query (Q4) execute on every request; they must be efficient. Recommended: ensure a composite index exists on `artists(is_approved, is_deleted, created_at)` if query performance is insufficient. Monitor post-launch.
- **NFR-3.** GA4 events must not block page rendering. Use beacon transport for navigation-critical events.

### Responsiveness

- **NFR-4.** All new homepage sections must work on both desktop and mobile layouts, respecting the existing sidebar (desktop) / bottom-nav (mobile) responsive pattern. Use Tailwind responsive breakpoints (`lg:` prefix) consistent with the existing layout.

### Accessibility

- **NFR-5.** All new elements must meet WCAG AA, consistent with recent accessibility work (commit `13ff97d`).
- **NFR-6.** All CTAs must be keyboard-navigable and have accessible names.
- **NFR-7.** Opportunity cards and stats must use semantic HTML (`<section>`, `<h2>`/`<h3>`, `<ul>`/`<li>`, `<a>`).
- **NFR-8.** Colour contrast must meet AA ratios for all new text and interactive elements.
- **NFR-9.** Artist images in opportunity cards and curated section must have `alt` text (artist name).

### SEO

- **NFR-10.** The homepage must retain its current SEO value. Search functionality and content discovery links (artist pages, lyric pages) must remain crawlable.
- **NFR-11.** Use semantic HTML heading hierarchy for new sections (`<h1>` for hero headline, `<h2>` for section headings).
- **NFR-12.** Opportunity card links to artist pages create additional internal linking, which is SEO-positive.

### Maintainability

- **NFR-13.** "Where help is needed" is entirely query-driven. The lyric count threshold (3) and recency window (90 days) should be defined as configuration constants in the service, not hardcoded in query strings.
- **NFR-14.** GA4 event names, entry points, source sections, opportunity types, and CTA names must be maintained as constants in a single module (see analytics-and-event-tracking.md, sections D and F).

### Data protection

- **NFR-15.** GA4 events must not transmit raw search queries, user-entered text, or PII. Only structured, controlled parameter values are sent (query_length, not query text).

---

## 9. Failure Modes & Edge Cases

### Opportunity cards: zero results

**Trigger:** All approved artists have 3+ approved lyrics.
**Behaviour:** Display a positive message instead of cards: "The archive is in great shape — but there are always more artists to add!" with a CTA to add an artist. The section heading still renders. No empty container is shown.

### Community impact stats: cache miss on cold start

**Trigger:** Application restart or first request.
**Behaviour:** Compute stats synchronously from the database on the first request. Cache the result. Subsequent requests use cache. Minimal latency impact (three COUNT queries on indexed columns).

### Community impact stats: database query failure

**Trigger:** Database connection error during stats computation (either on cold start or after cache expiry).
**Behaviour:** `IMemoryCache` removes expired entries, so there is no stale fallback. If the query fails and no cached value exists, set `CommunityImpact` to null on the view model. The view must handle null by hiding the community impact section entirely. Log the error via Sentry. The section will reappear on the next successful request.

### returnUrl lost during signup flow

**Trigger:** User goes through signup → confirm → login, and `returnUrl` is not preserved across all steps.
**Behaviour:** After login, user lands on `/` (homepage) instead of the intended destination. Analytics attribution for `contribution_flow_started` and `contribution_submitted` is lost for this session. Functionality is not impaired. This is acceptable degradation (Risk R5).

### returnUrl validation rejects URL with query params

**Trigger:** `Url.IsLocalUrl()` does not accept the encoded returnUrl.
**Behaviour:** After login, user lands on `/` instead of the destination. **Mitigation:** Test `Url.IsLocalUrl()` with URLs containing URL-encoded query parameters during development. If it fails, implement a TempData-based fallback: before redirecting to `/login`, store the full destination URL (with source params) in TempData. After login, check TempData first, then fall back to `returnUrl`.

### Opportunity card links to artist with no existing image

**Trigger:** Artist has `has_image = false`.
**Behaviour:** Display a placeholder/default artist avatar. The existing artist image helper already handles this case — reuse the same fallback.

### Curated discovery section: zero female artists

**Trigger:** No approved, non-deleted female artists exist.
**Behaviour:** Hide the curated discovery section entirely. Do not render an empty section.

### Small community impact numbers

**Trigger:** Stats are low (e.g., 12 lyrics, 5 artists, 3 contributors).
**Behaviour:** Always display the section (never suppress based on count). Use framing copy that contextualises small numbers positively: "X lyrics preserved and growing", "X artists documented so far", "X contributors and counting." The copy pattern works at any scale from 1 to 100,000+.

### GA4 gtag.js fails to load

**Trigger:** Network issue, ad blocker, or script error prevents gtag.js from loading.
**Behaviour:** All analytics events silently fail. No user-facing impact. The analytics JS module must check that `gtag` is defined before calling it, to prevent console errors.

---

## 10. Deployment & Operations

### Environments

Standard dev/prod pipeline. No new environment requirements.

### Configuration

| Setting | Location | Description |
|---|---|---|
| Opportunity card lyric threshold | Service constant | Default: 3. Number of approved lyrics below which an artist appears as an opportunity. |
| Opportunity card recency window | Service constant | Default: 90 days. Window for "newly added artists" query. |
| Opportunity card max results | Service constant | Default: 8. Maximum cards displayed. |
| Stats cache TTL | Service constant | Default: 30 minutes. Absolute expiration for community impact stats. |
| GA4 property ID | Existing in `_Layout.cshtml` | `G-3XMV1NEC87`. No change. |

### New DI registrations

| Service | Interface | Lifetime |
|---|---|---|
| `HomepageService` | `IHomepageService` | Scoped |
| `MemoryCache` | `IMemoryCache` | Singleton (via `AddMemoryCache()`) |

### Migrations

No database migrations required. The feature uses existing schema only. If post-launch query performance on Q1/Q2 requires a new index, that would be a separate operational decision.

### Secrets management

No new secrets. All external integrations (GA4, Cognito) use existing configuration.

---

## 11. Open Questions & Future Considerations

### Resolved questions

| # | Question | Resolution |
|---|---|---|
| OQ-1 (from REQUIREMENTS.md) | Does the auth redirect flow support preserving source context? | The application uses its own login form (not Cognito hosted UI), so the redirect flow is fully application-controlled. `returnUrl` with query params is the primary mechanism. Session/TempData is the fallback. |
| OQ-2 (from REQUIREMENTS.md) | Is GA4 loaded on all pages? | GA4 is loaded on all pages using `_Layout.cshtml`. It is NOT loaded on auth pages (`_AuthLayout.cshtml`). `sign_up_completed` fires on the post-auth landing page as best effort. |

### Deferred decisions

| # | Item | Notes |
|---|---|---|
| D-1 | Index optimisation for opportunity card queries | Monitor Q1/Q2 query performance post-launch. Add composite index on `artists(is_approved, is_deleted, created_at)` if needed. |
| D-2 | `sign_up_completed` reliability | If post-auth attribution proves unreliable, consider adding a server-side event via the GA4 Measurement Protocol as an alternative. |
| D-3 | Opportunity card lyric threshold tuning | The threshold of 3 may need adjustment as the archive grows. Make it a configuration constant for easy change. |
| D-4 | Caching for opportunity cards and curated section | v1 caches only community impact stats. If homepage load time is a concern post-launch, add caching for opportunity cards and the female artists query. |
| D-5 | `lyric_footer_add_lyric` entry point | The analytics spec defines an `add_lyric` CTA for the lyric detail page, but FR-28/FR-29 only require report copy strengthening. The analytics entry point is reserved for future use. |
| D-6 | Contributor count decoupling from point_events | v1 derives total contributors from `point_events` action types `ArtistApproved` and `LyricApproved`. If `action_type` semantics change or retroactive point consistency is in doubt, migrate to computing from approved submissions directly: `SELECT COUNT(DISTINCT user_id) FROM (SELECT user_id FROM artists WHERE is_approved = true AND is_deleted = false UNION SELECT user_id FROM lyrics WHERE is_approved = true AND is_deleted = false)`. The two approaches are expected to produce the same result under current data and assumptions, but this is not guaranteed — edge cases such as orphaned point events or bulk-approved records without corresponding point events could cause divergence. |

### Future enhancements (out of scope for v1)

- Homepage personalisation for authenticated users (welcome back, pending submissions).
- Featured contributors / top contributor rankings.
- "Artists missing a photo" module in "Where help is needed".
- Distributed caching if scaling to multiple instances.
- Server-side GA4 via Measurement Protocol for events that cannot fire client-side.
- Admin UI for curating homepage sections.

---

## Appendix: Traceability Matrix

| Requirement | Spec section |
|---|---|
| FR-1 through FR-3 (Hero area) | Section 6, P1 Hero area; Section 7, Flow 1 |
| FR-4, FR-5 (Inline explainer) | Section 6, P1 Section 3 |
| FR-6, FR-7 (Help grow the archive) | Section 6, P1 Section 2 |
| FR-8 through FR-12 (Where help is needed) | Section 5, Q1/Q2/composition; Section 6, P1 Section 4; Section 9, zero results |
| FR-13, FR-14 (Community impact) | Section 5, Q3/cache; Section 6, P1 Section 5; Section 7, Flow 5; Section 9, small numbers |
| FR-15, FR-16 (Curated discovery) | Section 5, Q4; Section 6, P1 Section 6 |
| FR-17 (Sidebar label) | Section 6, P6 |
| FR-18, FR-19 (Contribution copy, BB Points) | Section 6, P1 throughout |
| FR-20 through FR-24 (Auth redirect) | Section 7, Flow 2; Section 9, returnUrl fallbacks |
| FR-25 through FR-27 (Artist page CTA) | Section 6, P2; Section 7, Flow 3 |
| FR-28, FR-29 (Lyric page copy) | Section 6, P3 |
| FR-30 through FR-32 (GA4 events) | Section 6, GA4 integration; analytics-and-event-tracking.md |
| FR-33, FR-34 (Modules removed) | Section 6, P1 (old modules replaced by new sections) |
| NFR-1 through NFR-8 | Section 8 |
