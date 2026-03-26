## ADDED Requirements

### Requirement: IHomepageService interface with single orchestration method
The system SHALL provide a new service interface `IHomepageService` with method `GetHomepageViewModelAsync(bool isAuthenticated)` that returns a `HomepageViewModel`. The service SHALL be registered as Scoped in DI. The `isAuthenticated` parameter SHALL be passed through to the view model for CTA variant rendering.

#### Scenario: Homepage service is resolved and called for anonymous user
- **GIVEN** the DI container has `IHomepageService` registered as Scoped
- **WHEN** `HomeController.Index()` calls `GetHomepageViewModelAsync(isAuthenticated: false)`
- **THEN** a `HomepageViewModel` is returned with `IsAuthenticated = false`

#### Scenario: Homepage service is resolved and called for authenticated user
- **GIVEN** the DI container has `IHomepageService` registered as Scoped
- **WHEN** `HomeController.Index()` calls `GetHomepageViewModelAsync(isAuthenticated: true)`
- **THEN** a `HomepageViewModel` is returned with `IsAuthenticated = true`

### Requirement: HomepageViewModel replaces existing IndexViewModel
The system SHALL use a new `HomepageViewModel` with properties: `IsAuthenticated` (bool), `OpportunityCards` (IReadOnlyList of OpportunityCardViewModel), `CommunityImpact` (CommunityImpactStatsViewModel, nullable), and `FemaleArtists` (IEnumerable of RandomFemaleArtistItemViewModel).

#### Scenario: HomepageViewModel contains all required data sections
- **GIVEN** IHomepageService has executed all queries successfully
- **WHEN** the HomepageViewModel is composed
- **THEN** it SHALL contain OpportunityCards (list, possibly empty), CommunityImpact (stats object or null), and FemaleArtists (list, possibly empty)

### Requirement: OpportunityCardViewModel with artist data and opportunity type
The system SHALL provide an `OpportunityCardViewModel` with properties: `ArtistId` (int), `ArtistName` (string, full_name), `ArtistSlug` (string, primary slug), `HasImage` (bool), `ApprovedLyricCount` (int), and `OpportunityType` (string, either "artists_with_few_lyrics" or "new_artists_needing_lyrics").

#### Scenario: Opportunity card contains correct data shape
- **GIVEN** an approved artist with primary slug "sivan-perwer" and 1 approved lyric
- **WHEN** the artist appears as an opportunity card
- **THEN** the card SHALL have ArtistId, ArtistName matching full_name, ArtistSlug "sivan-perwer", HasImage reflecting the artist's image state, ApprovedLyricCount of 1, and OpportunityType set to one of the two allowed values

### Requirement: CommunityImpactStatsViewModel with three aggregate counts
The system SHALL provide a `CommunityImpactStatsViewModel` with properties: `TotalApprovedLyrics` (int), `TotalApprovedArtists` (int), and `TotalContributors` (int).

#### Scenario: Stats view model contains correct counts
- **GIVEN** the database has 50 approved non-deleted lyrics, 20 approved non-deleted artists, and 8 users with approved submissions
- **WHEN** community impact stats are computed
- **THEN** TotalApprovedLyrics SHALL be 50, TotalApprovedArtists SHALL be 20, and TotalContributors SHALL be 8

### Requirement: Q1 — artists with few lyrics query
The service SHALL query approved, non-deleted artists with fewer than 3 approved, non-deleted lyrics. Results SHALL be ordered by approved lyric count ascending (least lyrics first), then by artists.created_at descending (newest first as tiebreaker). Limit SHALL be configurable (default 8). Each result SHALL be tagged with opportunity_type "artists_with_few_lyrics".

#### Scenario: Q1 returns artists ordered by lyric count ascending
- **GIVEN** three approved non-deleted artists: artist A with 0 lyrics, artist B with 2 lyrics, artist C with 1 lyric
- **WHEN** Q1 executes
- **THEN** results SHALL be ordered [A, C, B] and each tagged with opportunity_type "artists_with_few_lyrics"

#### Scenario: Q1 excludes artists with 3 or more approved lyrics
- **GIVEN** an approved non-deleted artist with exactly 3 approved non-deleted lyrics
- **WHEN** Q1 executes
- **THEN** that artist SHALL NOT appear in results

#### Scenario: Q1 excludes deleted or unapproved artists
- **GIVEN** an artist with is_approved=false and 0 lyrics, and a deleted artist with 0 lyrics
- **WHEN** Q1 executes
- **THEN** neither artist SHALL appear in results

#### Scenario: Q1 only counts approved non-deleted lyrics
- **GIVEN** an approved non-deleted artist with 2 deleted lyrics and 1 approved non-deleted lyric
- **WHEN** Q1 executes
- **THEN** the artist's ApprovedLyricCount SHALL be 1

### Requirement: Q2 — newly added artists needing lyrics query
The service SHALL query approved, non-deleted artists where created_at is within the last 90 days and approved, non-deleted lyric count is fewer than 3. Results SHALL be ordered by created_at descending. Limit SHALL be configurable (default 4). Each result SHALL be tagged with opportunity_type "new_artists_needing_lyrics".

#### Scenario: Q2 returns only recently created artists
- **GIVEN** artist A created 30 days ago with 0 lyrics and artist B created 120 days ago with 0 lyrics
- **WHEN** Q2 executes
- **THEN** only artist A SHALL appear in results with opportunity_type "new_artists_needing_lyrics"

#### Scenario: Q2 excludes new artists with sufficient lyrics
- **GIVEN** an artist created 10 days ago with 3 approved non-deleted lyrics
- **WHEN** Q2 executes
- **THEN** that artist SHALL NOT appear in results

### Requirement: Opportunity card composition combines Q2 and Q1 with deduplication
The service SHALL compose opportunity cards by: (1) executing Q2 (limit 4), (2) executing Q1 (limit 8) excluding artist IDs already in Q2 results, (3) concatenating Q2 first then Q1, capping total at 8.

#### Scenario: Composition deduplicates and prioritizes new artists
- **GIVEN** Q2 returns artists [X, Y] and Q1 returns artists [Y, Z, W] (Y is in both)
- **WHEN** opportunity cards are composed
- **THEN** the final list SHALL be [X, Y, Z, W] with Y appearing only once (from Q2) and each retaining its original opportunity_type

#### Scenario: Composition caps at 8 total cards
- **GIVEN** Q2 returns 4 artists and Q1 returns 8 artists (no overlap)
- **WHEN** opportunity cards are composed
- **THEN** the final list SHALL contain exactly 8 cards (4 from Q2 + 4 from Q1)

### Requirement: Q3 — community impact stats with IMemoryCache caching
The service SHALL compute three scalar counts: total approved non-deleted lyrics, total approved non-deleted artists, and total contributors (distinct users with at least one ArtistApproved or LyricApproved point event, action_type IN (2, 4)). Results SHALL be cached in IMemoryCache with key "homepage:community_impact_stats" and 30-minute absolute expiration. On cache miss, compute from database and cache immediately.

#### Scenario: Stats are served from cache on cache hit
- **GIVEN** community impact stats were cached less than 30 minutes ago
- **WHEN** IHomepageService composes the homepage view model
- **THEN** stats SHALL be served from IMemoryCache without executing database queries

#### Scenario: Stats are computed and cached on cache miss
- **GIVEN** no cached stats exist (cold start or expired)
- **WHEN** IHomepageService composes the homepage view model
- **THEN** stats SHALL be computed from three COUNT queries, cached with 30-minute absolute expiration, and returned

#### Scenario: Stats cache expires after 30 minutes
- **GIVEN** stats were cached 31 minutes ago
- **WHEN** the next homepage request arrives
- **THEN** the cache SHALL miss and stats SHALL be recomputed from database

#### Scenario: Database failure during stats computation returns null
- **GIVEN** the database is unreachable during stats computation and no cached value exists
- **WHEN** IHomepageService attempts to compute stats
- **THEN** CommunityImpact on the view model SHALL be null and the error SHALL be logged via Sentry

### Requirement: Q4 — corrected female artists query
The existing `GetTopTenFemaleArtistsByLyricsCountAsync()` query SHALL be modified to add filters: `a.is_approved = true AND a.is_deleted = false` on artists, and `l.is_approved = true AND l.is_deleted = false` on lyrics in the count. No signature or return type change.

#### Scenario: Q4 excludes unapproved or deleted artists
- **GIVEN** a female artist with is_approved=false and 5 lyrics
- **WHEN** Q4 executes
- **THEN** that artist SHALL NOT appear in results

#### Scenario: Q4 only counts approved non-deleted lyrics
- **GIVEN** an approved non-deleted female artist with 3 approved lyrics and 2 deleted lyrics
- **WHEN** Q4 executes
- **THEN** that artist's lyric count SHALL be 3

### Requirement: Configurable query thresholds
The lyric count threshold (default 3), recency window (default 90 days), and max opportunity cards (default 8) SHALL be defined as configuration constants in the service, not hardcoded in query strings.

#### Scenario: Threshold constants are used in queries
- **GIVEN** the lyric threshold constant is set to 3
- **WHEN** Q1 or Q2 executes
- **THEN** the query SHALL use the constant value, not a hardcoded literal

### Requirement: IMemoryCache DI registration
The application SHALL register `IMemoryCache` as a Singleton via `builder.Services.AddMemoryCache()` if not already registered.

#### Scenario: IMemoryCache is available for injection
- **GIVEN** the application starts up
- **WHEN** HomepageService is resolved from DI
- **THEN** IMemoryCache SHALL be injected successfully
