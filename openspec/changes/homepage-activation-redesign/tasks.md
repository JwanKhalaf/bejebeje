## 1. View Models and Data Contracts

- [x] 1.1 Create `CommunityImpactStatsViewModel` with properties: `TotalApprovedLyrics` (int), `TotalApprovedArtists` (int), `TotalContributors` (int). Ref: SPEC §6 (New view models).
- [x] 1.2 Create `OpportunityCardViewModel` with properties: `ArtistId` (int), `ArtistName` (string), `ArtistSlug` (string), `HasImage` (bool), `ApprovedLyricCount` (int), `OpportunityType` (string). Ref: SPEC §6 (New view models).
- [x] 1.3 Create `HomepageViewModel` with properties: `IsAuthenticated` (bool), `OpportunityCards` (IReadOnlyList\<OpportunityCardViewModel\>), `CommunityImpact` (CommunityImpactStatsViewModel, nullable), `FemaleArtists` (IEnumerable\<RandomFemaleArtistItemViewModel\>). Ref: SPEC §6 (New view models).

## 2. IHomepageService and Data Queries

- [x] 2.1 Define `IHomepageService` interface with method `GetHomepageViewModelAsync(bool isAuthenticated)` returning `HomepageViewModel`. Ref: SPEC §6 (New service interface).
- [x] 2.2 Register `IHomepageService` → `HomepageService` as Scoped in DI. Ref: SPEC §10 (New DI registrations).
- [x] 2.3 Register `IMemoryCache` via `builder.Services.AddMemoryCache()` if not already registered. Ref: SPEC §10 (New DI registrations).
- [x] 2.4 Define service configuration constants: lyric threshold (default 3), recency window (default 90 days), max opportunity cards (default 8), stats cache TTL (default 30 minutes). Ref: SPEC §8 (NFR-13), §10 (Configuration).
- [x] 2.5 Implement Q1 query: approved, non-deleted artists with fewer than threshold approved non-deleted lyrics, ordered by lyric count ascending then created_at descending, limited to max cards. Tag results with opportunity_type "artists_with_few_lyrics". Ref: SPEC §5 (Q1).
- [x] 2.6 Implement Q2 query: approved, non-deleted artists created within recency window with fewer than threshold approved non-deleted lyrics, ordered by created_at descending, limited to 4. Tag results with opportunity_type "new_artists_needing_lyrics". Ref: SPEC §5 (Q2).
- [x] 2.7 Implement opportunity card composition: execute Q2 first, then Q1 excluding Q2 artist IDs, concatenate Q2 + Q1, cap at max cards. Ref: SPEC §5 (Opportunity card composition).
- [x] 2.8 Implement Q3 community impact stats: three COUNT queries (approved non-deleted lyrics, approved non-deleted artists, distinct contributors from point_events where action_type IN (2, 4)). Ref: SPEC §5 (Q3).
- [x] 2.9 Implement IMemoryCache caching for community impact stats with key "homepage:community_impact_stats" and 30-minute absolute expiration. On cache miss, compute from database and cache. On database failure, return null and log via Sentry. Ref: SPEC §5 (Cache state), §7 (Core Flow 5), §9 (Database query failure).
- [x] 2.10 Wire `GetHomepageViewModelAsync` to compose the full view model: opportunity cards, cached stats, female artists, and isAuthenticated flag. Ref: SPEC §6 (New service interface).

## 3. Corrected Female Artists Query

- [x] 3.1 Modify `GetTopTenFemaleArtistsByLyricsCountAsync()` in IArtistsService to add filters: `a.is_approved = true AND a.is_deleted = false` on artists, and `l.is_approved = true AND l.is_deleted = false` on lyrics in the count. No signature or return type change. Ref: SPEC §5 (Q4), §6 (Modified service method).

## 4. HomeController Changes

- [x] 4.1 Update `HomeController.Index()` to inject `IHomepageService`, resolve `isAuthenticated` from `User.Identity.IsAuthenticated`, call `GetHomepageViewModelAsync(isAuthenticated)`, and pass the `HomepageViewModel` to the view. Ref: SPEC §7 (Core Flow 1).

## 5. Homepage View — Hero Area

- [x] 5.1 Create hero area with headline (h1) framing cultural preservation, supporting text, and search input retaining existing search functionality. Ref: SPEC §6 (P1 Section 1), FR-1.
- [x] 5.2 Render primary CTA ("Add an artist" / "Join to add an artist") with auth-variant href. Authenticated: `/artists/new/selector`. Anonymous: `/login?returnUrl=...` with source params. Ref: SPEC §6 (P1 CTA behaviour), FR-2, FR-3.
- [x] 5.3 Render secondary CTA ("Contribute a lyric" / "Join to contribute a lyric") with auth-variant href. Authenticated: `/artists`. Anonymous: `/login?returnUrl=...` with source params. Ref: SPEC §6 (P1 CTA behaviour), FR-2, FR-3.

## 6. Homepage View — Help Grow the Archive

- [x] 6.1 Create "Help grow the archive" section (h2 heading) with task cards: add artist, add lyric, report issue. Each card has one-line description and link. Report issue card is instructional, linking to `/artists`. Ref: SPEC §6 (P1 Section 2), FR-6.
- [x] 6.2 Add "Create an account" task card visible only for anonymous users, linking to `/signup`. If sign-up attribution is being captured, include the source attribution values required by the chosen implementation. Hide for authenticated users. Ref: SPEC §6 (P1 Section 2), FR-7.

## 7. Homepage View — Inline Contribution Explainer

- [x] 7.1 Create inline explainer block with 4 steps: submit → review → publish + credit → earn BB Points. BB Points appear in final step as supporting motivation. No separate page. Ref: SPEC §6 (P1 Section 3), FR-4, FR-5.

## 8. Homepage View — Where Help Is Needed

- [x] 8.1 Create "Where help is needed" section (h2 heading) rendering up to 8 opportunity cards. Each card shows artist name and links to `/artists/{artistSlug}/lyrics`. Use invitation framing. Use semantic markup (section, ul/li, a). Artist images use alt text with artist name. Ref: SPEC §6 (P1 Section 4), FR-8, FR-10, FR-11, FR-12.
- [x] 8.2 Implement graceful degradation: when OpportunityCards list is empty, render section heading with positive fallback message "The archive is in great shape — but there are always more artists to add!" and link to add-artist flow. Ref: SPEC §9 (zero results), FR NFR-7.
- [x] 8.3 Handle artists with no image (has_image=false) by displaying placeholder/default avatar using the existing artist image helper fallback. Ref: SPEC §9 (no existing image).

## 9. Homepage View — Community Impact Stats

- [x] 9.1 Create community impact stats block showing three counters: approved lyrics, approved artists, contributors. Always displayed when CommunityImpact is not null. Use framing copy for small numbers (e.g. "X lyrics preserved and growing", "X artists documented so far", "X contributors and counting"). Ref: SPEC §6 (P1 Section 5), §9 (Small numbers), FR-13, FR-14.
- [x] 9.2 When CommunityImpact is null (database failure), hide the community impact section entirely. Ref: SPEC §9 (Database query failure).

## 10. Homepage View — Curated Discovery

- [x] 10.1 Create curated discovery section with editorial heading (e.g. "Women of Kurdish music") showing top female artists from FemaleArtists on the view model. Use h2 heading. Ref: SPEC §6 (P1 Section 6), FR-15, FR-16.
- [x] 10.2 When FemaleArtists list is empty, hide the curated discovery section entirely. Ref: SPEC §9 (Zero female artists).

## 11. Remove Old Homepage Modules

- [x] 11.1 Remove the "Recent submissions" module from the homepage view. Ref: FR-33.
- [x] 11.2 Remove the "Recently verified" module from the homepage view. Ref: FR-34.

## 12. Homepage Responsiveness and Accessibility

- [x] 12.1 Ensure all new homepage sections are responsive using Tailwind `lg:` breakpoints, working within the existing sidebar (desktop) / bottom-nav (mobile) pattern. Ref: SPEC §8 (NFR-4).
- [x] 12.2 Ensure all new homepage elements meet WCAG AA: keyboard-navigable CTAs with accessible names, semantic HTML (section, h1/h2, ul/li, a), colour contrast meeting AA ratios, alt text on artist images. Ref: SPEC §8 (NFR-5 through NFR-9).
- [x] 12.3 Ensure valid heading hierarchy: one h1 in hero, h2 for each section heading. Ref: SPEC §8 (NFR-11).

## 13. Artist Detail Page CTA

- [x] 13.1 Add "Know another lyric by this artist? Add one" CTA to the artist detail page (`Views/Lyric/ArtistLyrics.cshtml`), placed below artist info / above lyrics list. Copy and link only — no layout restructuring. Ref: SPEC §6 (P2), FR-25, FR-27.
- [x] 13.2 CTA link for authenticated users: `/artists/{artistSlug}/lyrics/new`. For anonymous users: `/login?returnUrl=...` with entry_point=artist_header_add_lyric and contribution_type=lyric encoded in the returnUrl. Ref: SPEC §6 (P2), FR-26.

## 14. Lyric Detail Page Copy Strengthening

- [x] 14.1 Add visible text "Spot an error? Report it" alongside the existing flag icon on the lyric detail page (`Views/Lyric/Lyric.cshtml`). Retain existing icon. Link target remains the existing report route. Copy change only — no layout restructuring. Ref: SPEC §6 (P3), FR-28, FR-29.

## 15. Sidebar Navigation Label

- [x] 15.1 In `Views/Shared/_Layout.cshtml`, change the sign-up label from "Sign up" to "Join as a contributor" for unauthenticated users. Link target remains `/signup`. Icon unchanged. Applies to both desktop sidebar and mobile bottom navigation. Ref: SPEC §6 (P6), FR-17.

## 16. Login and Signup Page Copy

- [x] 16.1 Add contribution-framing message to the login page (`Views/Account/Login.cshtml`): "Sign in to contribute lyrics, add artists, and get credit for approved contributions." Always displayed, not conditional on referrer. Ref: SPEC §6 (P4), FR-23.
- [x] 16.2 Add contribution-framing message to the signup page (`Views/Account/Signup.cshtml`) strengthening credit and BB Points angle. Always displayed. Ref: SPEC §6 (P5).
- [x] 16.3 Ensure login and signup cross-links preserve `returnUrl` when present, so users can move between auth pages without losing the intended post-auth destination. Ref: SPEC §7 (Core Flow 2 step 6).

## 17. Auth Redirect Flow — Source Context Preservation

- [x] 17.1 Ensure all anonymous CTA hrefs encode source attribution params (entry_point, contribution_type, source_section) into the returnUrl query string. Ref: SPEC §7 (Core Flow 2 step 4), FR-22.
- [x] 17.2 Verify `Url.IsLocalUrl()` accepts returnUrl values containing URL-encoded query parameters. If it fails, implement TempData-based fallback: store full destination URL in TempData before redirect to login, check TempData after login. Ref: SPEC §9 (returnUrl validation).
- [x] 17.3 Update the signup flow contract so `returnUrl` is preserved across GET `/signup`, POST `/signup`, GET `/signup/confirm`, POST `/signup/confirm`, and the redirect back to `/login`. Use explicit view-model properties, hidden fields, session, or TempData rather than relying on incidental behaviour. Ref: SPEC §7 (Core Flow 2 step 6).
- [x] 17.4 Update auth page view models/controllers as needed to carry `returnUrl` through the signup confirmation flow; the current `SignupViewModel` does not include `ReturnUrl`, so this must be added or replaced by an explicit preservation mechanism. Ref: `AccountController`, SPEC §7 (Core Flow 2 step 6).
- [x] 17.5 If direct signup-entry CTAs are attributed in v1, ensure the signup flow preserves `entry_point` and `source_section` in addition to any `returnUrl`, so `sign_up_completed` can use them on the first post-auth landing page. Ref: SPEC §6 (`sign_up_completed` best effort).

## 18. GA4 Analytics Constants Module

- [x] 18.1 Create analytics constants JavaScript module (e.g. `analytics-constants.js`) defining all event name constants: HOMEPAGE_SEARCH_SUBMITTED, HOMEPAGE_CTA_CLICKED, HOMEPAGE_OPPORTUNITY_CLICKED, AUTH_REDIRECT_INITIATED, CONTRIBUTION_FLOW_STARTED, CONTRIBUTION_SUBMITTED, SIGN_UP_COMPLETED, ARTIST_PAGE_CTA_CLICKED, LYRIC_PAGE_CTA_CLICKED. Ref: SPEC §6 (Analytics constants module), analytics-and-event-tracking.md §D, FR-31.
- [x] 18.2 Define constants for the `entry_point` values actually emitted in v1. At minimum: hero_add_lyric, hero_add_artist, help_grow_add_lyric, help_grow_add_artist, help_grow_report_issue, opportunity_artist_card, artist_header_add_lyric, lyric_side_report_issue, plus any additional join/signup entry points the implementation chooses to instrument. Ref: analytics-and-event-tracking.md §D.
- [x] 18.3 Define constants for the `source_section` values actually emitted in v1. At minimum: hero, hero_search, help_grow_archive, where_help_is_needed, artist_header, side_actions, plus any additional join/signup sections the implementation chooses to instrument. Ref: analytics-and-event-tracking.md §A.
- [x] 18.4 Define opportunity_type constants: artists_with_few_lyrics, new_artists_needing_lyrics. Define the `cta_name` constants actually emitted in v1, including: add_lyric, add_artist, report_issue, browse_artists, and join_contributor if any direct signup CTA is instrumented. Ref: analytics-and-event-tracking.md §B.2, §B.3.
- [x] 18.5 Define `destination_type` constants actually emitted in v1. At minimum: add_artist_page, login_page, signup_page, artist_index_page, and artist_page. Ref: analytics-and-event-tracking.md §A, SPEC §6 (GA4 integration).

## 19. GA4 Homepage Events

- [x] 19.1 Implement `homepage_search_submitted` event on homepage search submission. Parameters: source_page="home", source_section="hero_search", is_authenticated, ui_variant="homepage_v1", query_length (integer). No raw query text. Ref: analytics-and-event-tracking.md §B.1.
- [x] 19.2 Implement `homepage_cta_clicked` event on all homepage contribution CTA clicks. Parameters: source_page, source_section, is_authenticated, ui_variant, cta_name, entry_point, destination_type, auth_required. `destination_type` must describe the immediate clicked href in v1: `/login` => login_page, `/signup` => signup_page, `/artists/new/selector` => add_artist_page, `/artists` => artist_index_page. Use beacon transport. Ref: analytics-and-event-tracking.md §B.2.
- [x] 19.3 Implement `homepage_opportunity_clicked` event on "Where help is needed" card clicks. Parameters: source_page="home", source_section="where_help_is_needed", is_authenticated, ui_variant, opportunity_type, artist_id, entry_point="opportunity_artist_card", destination_type="artist_page". Use beacon transport. Ref: analytics-and-event-tracking.md §B.3.

## 20. GA4 Auth and Funnel Events

- [x] 20.1 Implement `auth_redirect_initiated` event on anonymous CTA clicks that trigger login redirect. Parameters: source_page, source_section, is_authenticated=false, ui_variant, entry_point, contribution_type, redirect_target, post_auth_destination, auth_required=true. Use beacon transport. Fire on homepage, artist detail, and lyric detail pages. Ref: analytics-and-event-tracking.md §B.4.
- [x] 20.2 Implement `contribution_flow_started` event on contribution form page load when attribution params exist in query string. Read entry_point, contribution_type, source_section from URL params. Parameters: source_page, source_section, is_authenticated, contribution_type, entry_point, flow_step. Do NOT fire if params are absent. Ref: analytics-and-event-tracking.md §B.5, SPEC §6 (contribution_flow_started attribution).
- [x] 20.3 Add hidden form fields for attribution params (entry_point, contribution_type, source_section) on artist, lyric, and report submission forms when those params are present in the query string. Ref: SPEC §6 (contribution_submitted attribution preservation).
- [x] 20.4 Update artist, lyric, and report submission controllers to read attribution hidden fields and store them in TempData before redirecting after successful submission. Ref: SPEC §6 (contribution_submitted attribution preservation).
- [x] 20.5 Implement `contribution_submitted` event on post-submission landing pages. Read attribution from TempData (rendered as data attributes or inline JSON by the view). Parameters: source_page, source_section, is_authenticated, contribution_type, entry_point, submission_status="submitted". Ref: analytics-and-event-tracking.md §B.6.
- [x] 20.6 Verify TempData persistence through the lyric submission double-redirect chain (POST → `/lyrics/like/{lyricId}` → lyric detail page). Ref: SPEC §6 (Note on lyric redirect chain).

## 21. GA4 Artist and Lyric Page Events

- [x] 21.1 Implement `artist_page_cta_clicked` event on the artist detail page CTA click. Parameters: source_page="artist_detail", source_section="artist_header", is_authenticated, cta_name="add_lyric", entry_point="artist_header_add_lyric", artist_id, auth_required. Use beacon transport. Ref: analytics-and-event-tracking.md §C.8.
- [x] 21.2 Implement `lyric_page_cta_clicked` event on the lyric detail page report CTA click. Parameters: source_page="lyric_detail", source_section="side_actions", is_authenticated, cta_name="report_issue", entry_point="lyric_side_report_issue", lyric_id, auth_required. Use beacon transport. Ref: analytics-and-event-tracking.md §C.9.

## 22. GA4 Sign Up Completed (Best Effort)

- [x] 22.1 Implement `sign_up_completed` event (best effort): on the first app-controlled page after sign-up/login, detect first-time login (e.g. OnSigningIn handler sets flag when new User record created), fire event with available source attribution. If attribution unavailable, skip event. Ref: analytics-and-event-tracking.md §B.7, SPEC §6 (sign_up_completed best effort).

## 23. GA4 Safety and Quality

- [x] 23.1 Add `typeof gtag !== 'undefined'` guard to all event-firing code to prevent console errors when gtag.js is blocked. Ref: SPEC §9 (gtag.js fails to load).
- [x] 23.2 Ensure no GA4 event blocks page rendering. Events fire asynchronously on user interaction only. Ref: SPEC §8 (NFR-3).
- [x] 23.3 Ensure no GA4 event transmits raw search queries, user-entered text, or PII. Only structured, controlled parameter values. Ref: SPEC §8 (NFR-15).
