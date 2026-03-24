## Context

The Bejebeje user profile feature (recently launched via the bb-points work) routes public profiles via `/profile/{username}`, where `username` is the raw Cognito `preferred_username`. This breaks for any user whose username contains spaces or trailing whitespace because browsers and web servers normalise whitespace in URL path segments. The current `ProfileController.Public` action does an exact-match query against the `username` column, which fails when the browser-normalised value doesn't match the stored value.

The existing codebase already has a proven slug pattern for artists and lyrics via the `NormalizeStringForUrl` extension method in `Bejebeje.Common.Extensions.StringExtensions`. This method produces lowercase, ASCII-safe, hyphen-separated strings. The same approach is applied to users.

Key current state:
- `User` entity has `Id`, `CognitoUserId`, `Username`, various point columns, `CreatedAt`, `ModifiedAt`.
- `BbContext` has unique indexes on `CognitoUserId` and `Username`.
- `ProfileController.Public` compares `preferred_username` claim to the route parameter for self-redirect detection — this is the string comparison that breaks with whitespace.
- `BbPointsService.GetPublicProfileDataAsync(string username)` queries `WHERE Username == username`.
- `BbPointsService.GetSubmitterPointsAsync` returns `SubmitterPointsViewModel` with `Username` but no slug.
- `LyricController.Lyric` builds the profile URL as `/profile/{submitterPoints.Username}`.
- The Retroactive console app uses raw SQL with Npgsql (no EF Core) and does not reference `Bejebeje.Common`.

## Goals / Non-Goals

**Goals:**
- All user profile pages reachable via URL-safe, deterministic slugs.
- Slug uniqueness enforced at the database level.
- Existing and future usernames trimmed of leading/trailing whitespace.
- Self-redirect check uses Cognito user ID comparison (reliable) instead of username string comparison (fragile).
- Data population for existing users handled by the Retroactive console app (run between two migrations).

**Non-Goals:**
- Custom or user-chosen slugs.
- Slug history or redirects from old URLs.
- Changes to artist or lyric slug infrastructure.
- Moving the duplicate username check to the sign-up flow (deferred).

## Decisions

### D-1: Reuse `NormalizeStringForUrl` for slug generation

**Choice**: Use the existing `StringExtensions.NormalizeStringForUrl()` method rather than creating a new slug algorithm.

**Rationale**: Consistency with the existing artist/lyric slug pattern. The method is well-tested and handles diacritics, spaces, and special characters. It already trims and lowercases input.

**Alternative considered**: A dedicated user slug generator — rejected because it would create inconsistency and duplicate logic.

### D-2: Two-migration deployment with Retroactive app in between

**Choice**: Migration 1 adds a nullable `slug` column. The Retroactive console app populates all slugs. Migration 2 adds NOT NULL + unique index.

**Rationale**: Cannot add a NOT NULL unique column to a table with existing rows in a single migration. The Retroactive app already exists as the data population mechanism for the bb-points feature and uses raw SQL, making it suitable for bulk operations.

**Alternative considered**: A single migration with a default value — rejected because slugs must be computed per-user (not a static default).

### D-3: Collision resolution via numeric suffix

**Choice**: When a generated slug already exists, append `-2`, `-3`, etc. until unique. Process users in `id ASC` order so earlier registrants get the cleaner slug.

**Rationale**: Simple, deterministic, and consistent with common slug collision patterns. The "different user" check in the collision loop ensures a user's own existing slug doesn't count as a collision during regeneration.

### D-4: Self-redirect via CognitoUserId comparison

**Choice**: Add `CognitoUserId` to `PublicProfileViewModel`. The controller compares it with the authenticated user's `sub` claim.

**Rationale**: The current approach compares `preferred_username` (from claims) with the route parameter (raw username). This fails when they differ due to whitespace. CognitoUserId is a stable, unique identifier that never changes.

**Alternative considered**: Look up the authenticated user's slug and compare with the route parameter — rejected because it would require an extra DB query. The `GetPublicProfileDataAsync` call already happens, so piggybacking the CognitoUserId on its result is zero-cost.

### D-5: Duplicate username detection at login time (FR-26/FR-27)

**Choice**: `EnsureUserExistsAsync` checks whether the trimmed username collides with another user's stored username before creating or updating. If collision: log warning, skip the operation.

**Rationale**: Prevents new users from claiming effectively identical usernames by adding whitespace. Since Cognito sign-up is external, this is the earliest application-level interception point.

### D-6: Kardox collision handling in data population

**Choice**: During the Retroactive Phase 2 sweep, if trimming a username would collide with an existing username (the Kardox case), skip the trim and leave the username as stored. Both users get distinct slugs via collision resolution.

**Rationale**: The existing unique constraint on `username` must not be violated. Slug differentiation handles URL routing. Only one known collision pair exists (user 26 "Kardox" and user 91 "Kardox ").

## Risks / Trade-offs

- **[Slug instability on rename]** → If a user renames in Cognito, their slug changes and old profile URLs break. Acceptable because the feature is new and no stable URLs exist. Future mitigation: slug history table (similar to `artist_slugs`).
- **[Race condition on slug creation]** → Two concurrent logins could attempt the same slug. Application-level collision resolution handles most cases; the database unique index is the final safety net. `SaveChangesAsync` throws `DbUpdateException`, caught by `OnTokenValidatedHandler`'s existing try/catch.
- **[Empty slug fallback]** → If `NormalizeStringForUrl` returns empty (username is entirely stripped characters), the fallback `user-{cognitoUserId[..8]}` produces a functional but non-descriptive slug. No current usernames trigger this.
- **[Retroactive ordering sensitivity]** → Slug assignment depends on processing order. Processing by `id ASC` is deterministic and gives earlier registrants cleaner slugs.

## Migration Plan

Deployment must follow this exact sequence (see SPEC §10):

1. Deploy all code changes (application + Retroactive console app).
2. Run Migration 1: adds nullable `slug` column to `users` table.
3. Run Retroactive console app: populates slugs and trims usernames for all existing users.
4. Verify: all users have non-null slugs.
5. Run Migration 2: sets `slug` to NOT NULL, adds unique index `ix_users_slug`.
6. Verify: application starts and profile pages work with slug-based URLs.

Rollback: if issues after Migration 2, drop the unique index and NOT NULL constraint. If issues after code deploy but before migrations, redeploy the previous code version.

## Open Questions

None — all questions were resolved during requirements and spec phases.
