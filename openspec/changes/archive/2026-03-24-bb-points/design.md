## Context

Bejebeje is a community-driven Kurdish lyrics archive built on ASP.NET Core MVC with PostgreSQL and AWS Cognito authentication. Users contribute artists, lyrics, and reports, but there is no incentive or recognition system. The existing data model includes `artists`, `lyrics`, `lyric_reports`, and `likes` tables — all with `user_id` columns storing Cognito sub claims as text. The `likes` table is not managed by EF Core. The architecture follows a Controllers → Services → Data Access → Database pattern.

The codebase currently has no local user table — user identity is resolved via Cognito on every request that needs it. The approval workflow uses `IApprovable` (boolean `IsApproved`). Report status uses a `ReportStatus` enum (Acknowledged=1, Dismissed=2). The `bejebeje.admin` project shares the same PostgreSQL database.

## Goals / Non-Goals

**Goals:**
- Introduce a points engine that awards points for defined actions with no noticeable latency impact
- Create a local users table that materializes per-category point totals and caches usernames
- Display points data across nav bar, profile pages, and lyric detail pages
- Enforce daily limits on submission points without blocking submissions
- Support retroactive calculation for all existing users
- Define a clear data contract for the admin project to award approval points

**Non-Goals:**
- Leaderboard (future phase)
- Configurable point values (fixed in code)
- Admin UI for daily limits (configuration only)
- Badges, achievements, or complex tier systems
- Unlike functionality (forward-compatible but not built)
- Changes to anonymous user experience

## Decisions

### D1: Materialized per-category totals on the users table

**Decision**: Store 6 per-category point columns on the `users` table instead of computing totals from `point_events` on every read.

**Rationale**: The own profile breakdown (FR-20) needs per-category points. Retroactive points (FR-34) must not create `point_events` entries. Materialized totals serve both retroactive and post-launch points, while the activity feed reads only from `point_events`. This also makes nav bar queries a single-row read (NFR-1).

**Alternative considered**: Compute totals by aggregating `point_events` per request. Rejected because retroactive points would either need synthetic events (violating FR-34) or a separate storage mechanism anyway.

### D2: Like-based points computed at read time, not stored

**Decision**: Like points are computed as `floor(COUNT(*) FROM likes WHERE user_id = X / 10)` on every display query. Not stored in a column.

**Rationale**: The `likes` table is not managed by EF Core and has no domain entity. Likes are state-based, not event-based. Computing at read time ensures accuracy and avoids maintaining a materialized count that could drift. At Bejebeje's current scale, this COUNT query on an indexed column is negligible (< 1ms). Ref: SPEC §5 Computed Values, §11 Future Consideration 7.

**Alternative considered**: Materialize like count on the `users` table. Deferred — unnecessary at current scale and adds complexity for like/unlike sync.

### D3: Daily limits count from source tables, not point_events

**Decision**: Daily submission counts are queried from the source tables (`artists`, `lyrics`, `lyric_reports`) by `user_id` and `created_at` on the UTC day. Not from `point_events`.

**Rationale**: A submission that exceeded the daily limit still exists in the source table but has no `point_event`. Counting from source tables ensures accuracy — every submission counts toward the limit regardless of whether it earned points. Ref: SPEC §5 Daily Limits Configuration.

### D4: View Component for nav bar points

**Decision**: Use an ASP.NET Core View Component (`BbPointsNavViewComponent`) invoked from `_Layout.cshtml` for the nav bar points display.

**Rationale**: View Components run server-side with their own logic, can access DI services, and render independently. This keeps the layout clean and the points logic encapsulated. The component returns empty content for anonymous users. Ref: SPEC §6 View Component.

### D5: TempData for daily limit messaging

**Decision**: Use ASP.NET Core TempData to pass points-earned/limit-reached data from controllers to the layout-level notification partial.

**Rationale**: TempData survives redirects (including double redirects like Create → Like → Lyric) and auto-expires after being read. This avoids modifying redirect targets or adding query string parameters. A shared `_PointsNotification.cshtml` partial in the layout reads the TempData keys. Ref: SPEC §6 TempData Contract.

### D6: User record sync on OnTokenValidated

**Decision**: Sync the local user record (create or update username) in the OIDC `OnTokenValidated` event handler.

**Rationale**: This runs once per login, guaranteeing the user record exists before any page renders. Subsequent page loads read from the local `users` table with no Cognito API calls. The `profile` OIDC scope should be requested to get `preferred_username` from claims; fallback to Cognito API call if needed. Ref: SPEC §6 Authentication Event, §7 Flow 1.

### D7: Standalone console app for retroactive calculation

**Decision**: Create a separate `Bejebeje.Retroactive` console application rather than embedding retroactive logic in the web app or a migration.

**Rationale**: The retroactive calculation is a one-time operation that queries all distinct users across multiple tables, makes Cognito API calls for username resolution, and performs bulk writes. Running this in a migration risks timeout and blocks deployment. A standalone app can be run manually with progress logging, is idempotent (UPSERT), and can be discarded after use. Ref: SPEC §7 Flow 12, §10 Retroactive Console Application.

### D8: UNIQUE constraint for idempotent point awards

**Decision**: Add a UNIQUE constraint on `(user_id, action_type, entity_id)` in `point_events` to prevent duplicate awards at the database level.

**Rationale**: Approval actions may be retried (admin double-click, timeout retry). The constraint ensures idempotency without application-level locking. The service uses INSERT ON CONFLICT DO NOTHING and skips the user total increment if the insert was a no-op. Ref: SPEC §8 NFR-4, §9 Duplicate Point Award Attempt.

## Risks / Trade-offs

- **[Dual points model complexity]** Event-based points (submissions/approvals) and state-based points (likes) use different calculation approaches. → Mitigation: Clear separation in code — 6 materialized columns for event-based, computed COUNT for likes. Ref: SPEC §3 Assumption 10.
- **[Retroactive photo detection reliability]** Historical `HasImage` flags may not accurately reflect submission-time state if modified after creation. → Mitigation: Validate historical data before running retroactive script. Flag ambiguous records for manual review. Ref: REQUIREMENTS §8 Risk 2.
- **[Race condition on daily limits]** Concurrent submissions may both pass the limit check. → Mitigation: Accepted — daily limit is a soft throttle, not a security boundary. At most 1 extra award. Ref: SPEC §9 Race Condition.
- **[Admin project dependency]** Approval points and notification emails require changes to the separate `bejebeje.admin` project. → Mitigation: Define a clear data contract (SPEC §6). MVC and admin deployments are independent. Short gap between deployments is acceptable.
- **[TempData survival through double redirect]** Lyric creation redirects through Create → Like → Lyric. TempData must survive both. → Mitigation: ASP.NET Core TempData persists until read. The Like action must not read BB Points TempData keys. Ref: SPEC §7 Flow 3.

## Migration Plan

1. Apply database migrations (new `users` and `point_events` tables). No existing tables modified — zero risk to running application.
2. Run `Bejebeje.Retroactive` console app against production database. Populates `users` with historical data. Sets `last_seen_points = total` to prevent false indicators.
3. Deploy updated Bejebeje.Mvc application. Enables BB Points display and submission point awarding.
4. Deploy updated bejebeje.admin project. Enables approval point awarding and notification emails.

Rollback: Remove the BB Points View Component invocation from `_Layout.cshtml` and the controller point-awarding calls. The new tables can remain — they are additive and do not affect existing functionality.
