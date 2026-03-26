## Architecture

The change is scoped to the MVC presentation layer and the service layer. No data model changes. No new external integrations.

### Component overview

```
HomeController.Index()
  └── IHomepageService.GetHomepageViewModelAsync(isAuthenticated)
        ├── Q1: artists with few lyrics (query per request)
        ├── Q2: newly added artists needing lyrics (query per request)
        ├── Q3: community impact stats (IMemoryCache, 30-min TTL)
        └── Q4: top female artists (corrected filters, query per request)
  └── Views/Home/Index.cshtml (redesigned, 6 sections)
        └── analytics.js (GA4 constants + event firing)

Modified pages:
  - Views/Lyric/ArtistLyrics.cshtml (add CTA)
  - Views/Lyric/Lyric.cshtml (copy strengthening)
  - Views/Account/Login.cshtml (contribution message)
  - Views/Account/Signup.cshtml (contribution message)
  - Views/Shared/_Layout.cshtml (sidebar label)
```

### System boundaries

- **In scope for code changes:** HomeController, IHomepageService (new), HomepageViewModel (new), homepage view, artist detail view, lyric detail view, login view, signup view, layout (sidebar), client-side analytics JS, existing `GetTopTenFemaleArtistsByLyricsCountAsync` query (filter correction).
- **Unchanged:** All submission forms, approval workflows, BB Points logic, Cognito integration, API endpoints, email service, image service, sitemap service.

Ref: SPEC.md §4 (System Architecture).

## Key design decisions

### 1. IHomepageService as orchestration layer

A new `IHomepageService` with a single method `GetHomepageViewModelAsync(bool isAuthenticated)` composes all homepage data. This keeps the controller thin and centralizes cache/query logic. Registered as Scoped in DI.

Ref: SPEC.md §6 (New service interface).

### 2. IMemoryCache for community impact stats

Community impact stats use `IMemoryCache` with 30-minute absolute expiration. On cache miss, stats are computed synchronously from three COUNT queries and cached immediately. Single-instance deployment means no distributed cache needed.

Cache key: `homepage:community_impact_stats`. Value type: `CommunityImpactStatsViewModel`.

Ref: SPEC.md §5 (Cache state), §7 (Core Flow 5), §8 (NFR-1).

### 3. Opportunity card composition

Q2 (new artists, limit 4) executes first. Q1 (all artists with few lyrics, limit 8) executes next, excluding Q2 artist IDs. Results concatenated: Q2 first, then Q1, capped at 8 total. Each card retains its `opportunity_type` tag for analytics.

Ref: SPEC.md §5 (Opportunity card composition).

### 4. Contributor count from point_events

Total contributors derived from `point_events` where `action_type IN (2, 4)` (ArtistApproved, LyricApproved). This is a known trade-off — it couples a homepage metric to the gamification event model but avoids a more expensive union query. The spec acknowledges potential divergence in edge cases (SPEC.md §11, D-6).

Ref: SPEC.md §5 (Q3).

### 5. Auth redirect via returnUrl

Anonymous CTA clicks for protected actions route through `/login?returnUrl=<encoded destination with source params>`. The login flow already supports `returnUrl` with `Url.IsLocalUrl()` validation. Source attribution params (`entry_point`, `contribution_type`, `source_section`) are encoded into the returnUrl query string.

Direct account-creation CTAs may link to `/signup` instead, but that path needs an explicit preservation mechanism for any `returnUrl` and source attribution values that must survive the signup-confirm-login chain. The current codebase does not preserve those values automatically.

TempData fallback: if `Url.IsLocalUrl()` rejects encoded URLs, store full destination in TempData before redirect.

Ref: SPEC.md §7 (Core Flow 2), §9 (returnUrl validation).

### 6. GA4 event transport

Use `navigator.sendBeacon` transport (via gtag `transport_type: 'beacon'`) for events firing immediately before navigation (e.g., `homepage_cta_clicked`, `auth_redirect_initiated`). All event code checks `typeof gtag !== 'undefined'` before calling.

Ref: SPEC.md §6 (GA4 integration), §9 (gtag.js fails to load).

### 7. Attribution through submission redirect chains

`contribution_flow_started` fires on form page load (reads attribution from query params). `contribution_submitted` fires on final landing page after submission (reads attribution from TempData). Form pages include attribution as hidden fields. Controllers store attribution in TempData before redirect.

The lyric submission has a double-redirect chain (POST → `/lyrics/like/{lyricId}` → final page). TempData must survive this. Engineers should verify early.

Ref: SPEC.md §6 (contribution_submitted attribution preservation).

## Data flow

### Homepage request

1. Request → `HomeController.Index()`
2. Controller resolves `isAuthenticated` from `User.Identity.IsAuthenticated`
3. Controller calls `IHomepageService.GetHomepageViewModelAsync(isAuthenticated)`
4. Service executes Q2, Q1 (excluding Q2 IDs), composes opportunity cards (cap at 8)
5. Service checks `IMemoryCache` for stats; on miss, executes Q3 and caches
6. Service calls `GetTopTenFemaleArtistsByLyricsCountAsync()` (with corrected filters)
7. Service returns `HomepageViewModel`
8. Controller returns `View(model)`
9. View renders 6 sections with auth-variant CTAs based on `IsAuthenticated`

Ref: SPEC.md §7 (Core Flow 1).

### Anonymous CTA click

1. User clicks CTA → JS fires `homepage_cta_clicked` (beacon transport)
2. If anonymous → JS fires `auth_redirect_initiated` (beacon transport)
3. Browser navigates to `/login?returnUrl=...`
4. User logs in → redirect to destination with source params
5. Destination page JS fires `contribution_flow_started`
6. User submits → controller stores attribution in TempData → redirects
7. Final page JS fires `contribution_submitted` (reads TempData)

Ref: SPEC.md §7 (Core Flow 2).

## External dependencies

| Dependency | Type | Change required |
|---|---|---|
| PostgreSQL | Database | No schema changes. New read queries only. |
| AWS Cognito | Authentication | No changes. Existing returnUrl mechanism used. |
| Google Analytics 4 | Client-side tracking | New custom events pushed via existing gtag.js. |

## DI registrations

| Service | Interface | Lifetime |
|---|---|---|
| `HomepageService` | `IHomepageService` | Scoped |
| `MemoryCache` | `IMemoryCache` | Singleton (via `AddMemoryCache()`) |

## Configuration constants

| Setting | Default | Description |
|---|---|---|
| Lyric threshold | 3 | Approved lyrics below which an artist appears as opportunity |
| Recency window | 90 days | Window for "newly added artists" query |
| Max opportunity cards | 8 | Maximum cards displayed |
| Stats cache TTL | 30 minutes | Absolute expiration for community impact stats |
