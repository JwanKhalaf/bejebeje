# Requirements

## 1. Overview

### Problem Statement

The current Bejebeje homepage is optimised for a platform with visible community momentum. Bejebeje does not yet have that momentum. The homepage leads with activity-based modules (recent submissions, recently verified, popular female artists) that signal inactivity on a quiet platform, weakening social proof and discouraging participation.

The homepage currently helps users find content but does nothing to recruit visitors into contribution. It has no participation thesis, no contribution prompts, no bridge from reading to acting, and no visible evidence of community stewardship.

### Goals & Objectives

1. Reposition the homepage from a passive browse surface to an activation surface that converts cultural interest into first contributions.
2. Replace stale activity-led modules with contribution-led modules that work regardless of platform velocity.
3. Make contribution feel visible, meaningful, and culturally significant — not like filling database gaps.
4. Give anonymous visitors a clear next step toward participation.
5. Stop the homepage from signalling inactivity.

### Success Criteria

Primary metrics (measured via GA4):

- Increase in homepage → sign-up click rate.
- Increase in homepage → contribution CTA click rate.
- Increase in homepage → contribution flow start rate.
- Increase in percentage of users who start a contribution flow after landing on the homepage.
- Increase in approved first-time contributions per week after launch.

Secondary metrics:

- Search usage from the homepage does not materially decline.
- Bounce rate from homepage improves.
- More users reach artist pages with low-lyric counts from homepage modules.

---

## 2. Stakeholders & Users

### Stakeholder List

| Stakeholder | Role |
|---|---|
| Site admins | Approve/reject submitted content; maintain platform quality |
| Product owner | Defines engagement strategy; measures activation success |

### User Roles & Descriptions

| Role | Description |
|---|---|
| Anonymous visitor | A user who has not signed up or logged in. Can search and browse approved content. Cannot contribute. |
| Authenticated contributor | A logged-in user who can submit artists, submit lyrics, like lyrics, and report lyrics. All submissions require admin approval. |

---

## 3. Scope

### In Scope

1. **Homepage redesign** — new layout, new sections, new copy, replacing existing modules.
2. **Small CTA addition on artist detail page (required in v1)** — a lightweight contribution prompt: "Know another lyric by this artist? Add one".
3. **Small copy strengthening on lyric detail page (required in v1)** — reinforce existing report/contribution affordances with clearer copy: "Spot an error? Report it".
4. **Inline homepage explainer** — a compact block explaining how contributions work (submit → review → publish → credit), replacing a standalone page.
5. **"Where help is needed" module** — showing contribution opportunities from existing data.
6. **"Community impact" stats block** — cached aggregate counts.
7. **Contribution-focused copy and CTA labels** — across the homepage, sidebar, and sign-up entry points.
8. **Authenticated redirect flow for anonymous CTA clicks** — preserving source context through login/sign-up.
9. **GA4 event instrumentation** — a defined set of activation-focused custom events.

### Out of Scope

| Item | Reason |
|---|---|
| A/B testing infrastructure | Build the new homepage outright; compare before/after via GA4 |
| Homepage personalisation (welcome back, pending submissions, activity) | Phase 2 |
| Featured contributors / top contributor rankings | Introduces ranking logic, fairness questions, and profile visibility decisions not needed for v1 |
| Author page changes | Not needed for v1 activation |
| Public surfacing of reported lyrics | Moderation-sensitive; must remain internal |
| New taxonomy fields (region, dialect, era) | Data model change not justified for v1 |
| Admin UI for curated homepage sections | Adds operational burden; use query-driven or hardcoded approach |
| Cognito sign-up form redesign | Out of scope; only labels and surrounding copy change |
| Custom analytics infrastructure | Use existing GA4 setup |
| "Artists missing a photo" in "Where help is needed" | Secondary to the lyrics contribution loop; photo upload scope and permissions are a separate conversation |
| Full-site engagement redesign | This phase is homepage-first activation work |

---

## 4. Functional Requirements

### Hero Area

- **FR-1.** The homepage must display a hero area containing:
  - a headline that frames the platform's purpose around preserving Kurdish lyrics collaboratively,
  - supporting text that explains what users can do (search, discover, contribute),
  - a search input for finding artists and lyrics (retaining current search functionality),
  - a primary CTA to add an artist,
  - a secondary CTA to contribute a lyric.

- **FR-2.** For anonymous users, the hero CTAs must read in contribution-inviting language (e.g. "Sign up to add an artist", "Sign up to contribute a lyric").

- **FR-3.** For authenticated users, the hero CTAs must link directly to the existing submission flows (e.g. "Add an artist", "Contribute a lyric").

### How Contributions Work (Inline Explainer)

- **FR-4.** The homepage must include a compact inline explainer block that communicates the contribution workflow:
  1. Submit content (artists or lyrics).
  2. Admins review submissions.
  3. Approved contributions appear publicly and are credited to the contributor.
  4. Contributors earn BB Points for approved submissions.

- **FR-5.** This explainer must not be a separate page. It must be rendered inline on the homepage.

### "Help Grow the Archive" Section

- **FR-6.** The homepage must display a task-oriented section presenting clear contribution options. Each option must include a one-line description of the action. The options are:
  - Add a missing artist.
  - Add lyrics for an existing artist.
  - Report an issue with a lyric.
  - Create an account to get credit for contributions (anonymous users only).

- **FR-7.** For authenticated users, the "Create an account" option must be replaced or hidden, showing only actionable contribution options.

### "Where Help Is Needed" Section

- **FR-8.** The homepage must display a "Where help is needed" section showing specific contribution opportunities derived from existing data.

- **FR-9.** The section must include the following opportunity types at launch:
  - **Artists with few lyrics** — approved artists that have fewer than 3 approved lyrics.
  - **Newly added artists needing lyrics** — artists approved within the last 90 days that have fewer than 3 approved lyrics.

- **FR-10.** Each opportunity card must display the artist name, and link to the artist detail page where the user can then add a lyric.

- **FR-11.** Opportunities must be framed as invitations (e.g. "Help complete these artist pages"), not as deficiency statements (e.g. not "These pages are empty").

- **FR-12.** The section must display a maximum of 8 opportunity cards.

### "Community Impact" Stats Block

- **FR-13.** The homepage must display a community impact block showing aggregate platform statistics:
  - Total number of approved lyrics.
  - Total number of approved artists.
  - Total number of contributors. A contributor is defined as a user with at least one approved artist submission or at least one approved lyric submission.

- **FR-14.** These counts must be cached and periodically refreshed, not computed live on every homepage request. The community impact block must always be displayed regardless of how small the counts are — framing copy must make small numbers feel purposeful (e.g. "X lyrics preserved and growing").

### Curated Discovery Section

- **FR-15.** The homepage must include one curated discovery section based on existing queryable data. For v1, this is the existing "popular female artists" query (top artists by lyric count filtered by sex).

- **FR-16.** The curated section must have editorial framing (e.g. "Women of Kurdish music") rather than appearing as a raw data listing.

### Contribution-Focused Copy

- **FR-17.** The sidebar navigation label for sign-up must be changed from "Sign up" to "Join as a contributor".

- **FR-18.** Contribution framing copy must appear in visible places on the homepage, conveying:
  - that contributions help preserve Kurdish musical heritage,
  - that contributors receive credit on the site,
  - that contributors earn BB Points.

- **FR-19.** BB Points must appear on the homepage only as supporting motivation (e.g. "Earn BB Points for approved contributions"), not as the primary driver. The emotional driver must be contribution and recognition.

### Authentication Redirect for Anonymous Users

- **FR-20.** When an anonymous user clicks any contribution CTA (add artist, add lyric, or join/sign-up), they must be redirected to the login/sign-up page.

- **FR-21.** After successful authentication, the user must be redirected back to the intended destination (e.g. the add-artist form or add-lyric form).

- **FR-22.** Source context must be preserved through the redirect flow (e.g. via a query parameter like `?source=homepage_add_artist`) so that GA4 can attribute the sign-up to a specific homepage CTA.

- **FR-23.** The login/sign-up entry point must display a short explanatory message, such as: "Create an account to contribute lyrics, add artists, and get credit for approved contributions."

- **FR-24.** No modal must be used for this flow in v1. Standard page redirect is required.

### Artist Detail Page — Small CTA Addition

- **FR-25.** The artist detail page must display a lightweight contribution CTA such as "Know another lyric by this artist? Add one", linking to the add-lyric form for that artist.

- **FR-26.** This CTA must follow the same authentication redirect behaviour as homepage CTAs (FR-20 through FR-22) for anonymous users.

- **FR-27.** This must be a copy/link addition only — no major layout changes to the artist detail page.

### Lyric Detail Page — Copy Strengthening

- **FR-28.** The lyric detail page must strengthen existing reporting/contribution affordances with clearer copy (e.g. "Spot an error? Report it").

- **FR-29.** This must be a copy change only — no major layout changes to the lyric detail page.

### GA4 Event Instrumentation

The full GA4 event schema — including event definitions, parameter tables, naming conventions, entry point enums, example payloads, and implementation rules — is documented in **[analytics-and-event-tracking.md](analytics-and-event-tracking.md)**.

The requirements below summarise the scope. The analytics spec is the authoritative reference for implementation.

- **FR-30.** The following custom GA4 events must be instrumented for v1:

| Event Name | Trigger | Full Spec |
|---|---|---|
| `homepage_search_submitted` | User submits a search from the homepage | Section B.1 |
| `homepage_cta_clicked` | User clicks a contribution CTA on the homepage | Section B.2 |
| `homepage_opportunity_clicked` | User clicks an item in "Where help is needed" | Section B.3 |
| `auth_redirect_initiated` | Anonymous user is redirected to login/sign-up from a contribution CTA | Section B.4 |
| `contribution_flow_started` | User lands in a submission flow originating from the homepage | Section B.5 |
| `contribution_submitted` | User completes a submission | Section B.6 |
| `sign_up_completed` | User completes sign-up and returns to app | Section B.7 |

- **FR-30a.** The following events must also be instrumented for v1, corresponding to the artist detail page and lyric detail page CTA additions (FR-25 through FR-29):

| Event Name | Trigger | Full Spec |
|---|---|---|
| `artist_page_cta_clicked` | User clicks a contribution CTA on the artist detail page | Section C.8 |
| `lyric_page_cta_clicked` | User clicks a contribution CTA on the lyric detail page | Section C.9 |

- **FR-31.** Event names and parameter values must be consistent and stable. They must be defined up front and not changed without coordination. All event names, `entry_point` values, `source_section` values, `opportunity_type` values, and `cta_name` values must be maintained as constants in code (see analytics spec, sections D and F).

- **FR-32.** Source context must be preserved through auth redirects so that `contribution_flow_started` and `contribution_submitted` can attribute the originating homepage CTA. At minimum, `entry_point`, `contribution_type`, and `post_auth_destination` must survive the redirect round-trip (see analytics spec, section F, Rule 3).

### Modules Removed

- **FR-33.** The "Recent submissions" module must be removed from the homepage.

- **FR-34.** The "Recently verified" module must be removed from the homepage.

---

## 5. Non-Functional Requirements

- **NFR-1. Performance.** The community impact stats block (FR-13) must use cached data. Homepage load time must not increase materially compared to the current homepage. Target: homepage renders in under 2 seconds on a standard connection.

- **NFR-2. Responsiveness.** The redesigned homepage must work on both desktop and mobile layouts. The current application uses a sidebar on desktop and bottom navigation on mobile — the new homepage sections must respect this existing responsive pattern.

- **NFR-3. Accessibility.** All new homepage elements must meet WCAG AA standards, consistent with the recent accessibility work (commit 13ff97d). All CTAs must be keyboard-navigable. All images must have alt text. Colour contrast must meet AA ratios.

- **NFR-4. SEO.** The homepage must retain its current SEO value. Search functionality and content discovery links must remain crawlable. Semantic HTML headings must be used for new sections.

- **NFR-5. Maintainability.** The "Where help is needed" section must be driven by database queries against existing fields, not by hardcoded content that requires code changes to update.

- **NFR-6. Cache freshness.** Community impact stats must refresh every 30 minutes. Stale stats within the 30-minute window are acceptable; stats from hours or days ago are not.

- **NFR-7. Graceful degradation.** If the "Where help is needed" query returns zero results (e.g. every artist has many lyrics), the section must either hide itself or display a positive message (e.g. "The archive is in great shape — but there are always more artists to add!"). It must not render an empty or broken section.

- **NFR-8. Analytics reliability.** GA4 events must fire reliably on user interaction. Events must not block page rendering or degrade user experience.

---

## 6. Data & Integrations

### Data Inputs

| Data | Source | Notes |
|---|---|---|
| Artists with few lyrics | `artists` + `lyrics` tables (existing) | Query approved, non-deleted artists with fewer than 3 approved, non-deleted lyrics |
| Newly added artists needing lyrics | `artists` + `lyrics` tables (existing) | Query approved, non-deleted artists where `created_at` is within the last 90 days and approved lyric count is fewer than 3, ordered by `created_at` DESC |
| Total approved lyrics count | `lyrics` table (existing) | COUNT where `is_approved = true` and `is_deleted = false` |
| Total approved artists count | `artists` table (existing) | COUNT where `is_approved = true` and `is_deleted = false` |
| Total contributors count | `users` + `point_events` tables (existing) | COUNT of users with at least one `ArtistApproved` or `LyricApproved` point event |
| Popular female artists | Existing `GetTopTenFemaleArtistsByLyricsCountAsync()` query | No change needed |
| User authentication state | AWS Cognito (existing) | Used to toggle anonymous vs authenticated CTA variants |

### Data Outputs

| Output | Destination | Notes |
|---|---|---|
| GA4 custom events | Google Analytics 4 | Client-side event push via gtag.js or equivalent |
| Source context parameter | Login/sign-up redirect URL | Query parameter preserving CTA origin for post-auth redirect and GA4 attribution |

### External Integrations

| System | Integration Type | Notes |
|---|---|---|
| Google Analytics 4 | Client-side event tracking | Existing GA4 setup; add custom events only. Full event schema in [analytics-and-event-tracking.md](analytics-and-event-tracking.md) |
| AWS Cognito | Authentication redirect | Existing integration; must support return URL with source parameter |

---

## 7. Constraints & Assumptions

### Constraints

1. **No new database fields or tables.** All homepage data must be derived from existing schema (artists, lyrics, users, point_events, artist_slugs, lyric_slugs).
2. **No new admin tooling.** Curated sections and "Where help is needed" must be query-driven or config-driven, not admin-UI-driven.
3. **No Cognito sign-up form changes.** Only labels and surrounding copy in the application are in scope; the hosted Cognito authentication form itself is not modified.
4. **No new standalone pages.** The "how contributions work" content is inline on the homepage, not a separate route.
5. **Existing layout structure.** The desktop sidebar + mobile bottom navigation layout must be preserved. Homepage changes are to the main content area.
6. **Tailwind CSS 4.0.3.** Styling must use the existing Tailwind setup.

### Assumptions

1. The existing GA4 setup supports custom event tracking (gtag.js or equivalent is already loaded on the site).
2. The Cognito redirect flow supports a return URL parameter that can include source context.
3. The existing `GetTopTenFemaleArtistsByLyricsCountAsync()` query can continue to serve the curated discovery section without modification.
4. Artist and lyric submission flows (forms, validation, BB Points awarding) remain unchanged — the homepage only links to them.
5. Approval workflow remains manual and unchanged — the homepage reflects current approved content and does not introduce automated approval.
6. The threshold of fewer than 3 approved lyrics (FR-9) is appropriate for the current archive size and may be adjusted post-launch if needed.

---

## 8. Risks & Considerations

### Identified Risks

| # | Risk | Impact | Mitigation |
|---|---|---|---|
| R1 | "Where help is needed" may expose content gaps in a way that makes the archive feel incomplete | Users may perceive the platform as empty rather than as a place they can help | Frame all opportunities as invitations ("Help complete these artist pages"), not deficiency statements. Use positive, action-oriented copy. |
| R2 | Too many CTAs on the homepage could overwhelm casual visitors who just want to search | Search-only users may bounce if the page feels pushy | Keep search prominent at the top of the hero. Contribution modules sit below search. Measure search usage post-launch to confirm no decline. |
| R3 | Anonymous users hitting auth redirects may abandon the flow | Contribution intent is lost at the sign-up wall | Preserve return URL so users land at their intended destination after auth. Display clear explanatory copy on the auth page. Track `auth_redirect_initiated` events to monitor drop-off. |
| R4 | Community impact stats may look small and underwhelming on an early-stage platform | Small numbers could signal weakness rather than community | Use framing copy that makes small numbers feel purposeful (e.g. "X lyrics preserved and growing" rather than just "X lyrics"). The block is always shown — suppression logic is not used. |
| R5 | Source context may be lost during the Cognito redirect flow | GA4 attribution of sign-ups to specific CTAs would be unreliable | Design for full source preservation (`entry_point`, `contribution_type`, `post_auth_destination`) through the Cognito redirect. If Cognito cannot reliably preserve source context, accept degraded attribution as a fallback — do not block the release. The homepage redesign is more important than perfect analytics attribution. |
| R6 | Cached community impact stats could show stale or zero counts after a cache miss or cold start | Users would see missing or incorrect data | Implement a fallback: if cache is empty, either compute on-demand once or hide the section until the cache is warm. |

---

## 9. Open Questions

| # | Question | Owner | Impact |
|---|---|---|---|
| OQ-1 | Does the existing Cognito redirect flow support preserving a custom source parameter (`entry_point`, `contribution_type`, `post_auth_destination`) through the full auth round-trip? If not, what is the best fallback mechanism? | Architect | Affects FR-22 and FR-32. Product decision: do not block the release if full preservation is not feasible — degraded attribution is acceptable (see risk R5). |
| OQ-2 | Is GA4 (gtag.js) already loaded on all pages, or only on specific pages? | Architect | Affects FR-30 implementation approach — determines whether page-level events on artist/lyric pages need additional plumbing. |

---

## 10. UI Deliverable Inventory

The following distinct pages or screens must be created or modified. This is scope enumeration, not UI design.

### New / Redesigned

| # | Page | Purpose | Key Actions / Data |
|---|---|---|---|
| P1 | **Homepage (redesigned)** | Primary activation surface; search, contribute, discover | Search input; hero with contribution CTAs; "Help grow the archive" task cards; "Where help is needed" opportunity cards; community impact stats; inline contribution explainer; curated discovery section (women of Kurdish music) |

### Modified (Small Changes, Required in v1)

| # | Page | Change | Key Actions / Data |
|---|---|---|---|
| P2 | **Artist detail page** | Add lightweight contribution CTA | "Know another lyric by this artist? Add one" link to add-lyric form; auth redirect for anonymous users |
| P3 | **Lyric detail page** | Strengthen report/contribution copy | "Spot an error? Report it" copy on existing report affordance |
| P4 | **Login / sign-up entry point** | Add contextual contribution messaging | Short explanatory message: "Create an account to contribute lyrics, add artists, and get credit for approved contributions" |
| P5 | **Sidebar navigation** | Update sign-up label | Change "Sign up" to "Join as a contributor" |

### Unchanged

| Page | Reason |
|---|---|
| Author detail page | Explicitly out of scope for this phase |
| Artist submission form | Existing flow; homepage only links to it |
| Lyric submission form | Existing flow; homepage only links to it |
| Report form | Existing flow; lyric page only links to it |
| Profile pages (own + public) | No changes in this phase |
| Cognito hosted sign-up form | Out of scope; only surrounding application copy changes |
