## 1. Domain & Data Model

- [x] 1.1 Add `Slug` property (type `string`) to the `User` entity in `Bejebeje.Domain/User.cs`. Ref: SPEC §5 User Entity Changes.
- [x] 1.2 Add unique index configuration for `User.Slug` in `BbContext.OnModelCreating`: `entity.HasIndex(e => e.Slug).IsUnique().HasDatabaseName("ix_users_slug")` inside the existing `builder.Entity<User>` block. Ref: SPEC §5 Database Schema Changes.
- [x] 1.3 **[BLOCKED — user must create migration]** Migration 1 (AddSlugToUsers): add nullable `slug` column (text, nullable) to `users` table. No constraints yet. Ref: SPEC §5 Migration 1.
- [x] 1.4 **[BLOCKED — user must create migration]** Migration 2 (MakeSlugNotNullAndUnique): alter `slug` to NOT NULL, add unique index `ix_users_slug`. Run only after Retroactive console app populates all slugs. Ref: SPEC §5 Migration 2.

## 2. ViewModel Changes

- [x] 2.1 Add `Slug` property (type `string`) to `SubmitterPointsViewModel` in `Bejebeje.Models/BbPoints/SubmitterPointsViewModel.cs`. Ref: SPEC §6 ViewModel Changes.
- [x] 2.2 Add `CognitoUserId` property (type `string`) to `PublicProfileViewModel` in `Bejebeje.Models/BbPoints/PublicProfileViewModel.cs`. Ref: SPEC §6 ViewModel Changes.

## 3. Slug Generation Logic in BbPointsService

- [x] 3.1 Add a private helper method to `BbPointsService` that implements the slug collision resolution algorithm: apply `NormalizeStringForUrl()` to the trimmed username, use fallback `user-{cognitoUserId[..Min(8, len)]}` if empty, then loop appending `-2`, `-3`, etc. until unique (checking against `_context.Users` excluding the current user's own record). Ref: SPEC §7 Flow 7.
- [x] 3.2 Log at warning level when the empty-slug fallback is triggered. Ref: SPEC §8 Observability.

## 4. EnsureUserExistsAsync — New User Path

- [x] 4.1 Trim `username` parameter at the start of `EnsureUserExistsAsync`: `var trimmedUsername = username.Trim()`. Use `trimmedUsername` for all subsequent operations. Ref: SPEC §7 Flow 1 step 4.
- [x] 4.2 Add duplicate username check before creating a new user: query `_context.Users.AnyAsync(u => u.Username == trimmedUsername && u.CognitoUserId != cognitoUserId)`. If true, log a warning and return null. Ref: SPEC §7 Flow 1 step 6, SPEC §9 E-1.
- [x] 4.3 Generate slug from `trimmedUsername` using the collision resolution helper (task 3.1) when creating a new user. Set `user.Slug = generatedSlug`. Ref: SPEC §7 Flow 1 steps 7-9.
- [x] 4.4 Store `Username = trimmedUsername` (not the raw parameter) on the new user record. Ref: SPEC §7 Flow 1 step 9.
- [x] 4.5 Log at debug level when a slug is generated for a new user. Ref: SPEC §8 Observability.

## 5. EnsureUserExistsAsync — Returning User Path

- [x] 5.1 Change the username comparison for detecting changes to compare trimmed values: `if (user.Username != trimmedUsername)` instead of `if (user.Username != username)`. This prevents unnecessary updates when Cognito sends trailing whitespace. Ref: SPEC §7 Flow 2 step 6.
- [x] 5.2 Add duplicate username check before updating an existing user's username: query `_context.Users.AnyAsync(u => u.Username == trimmedUsername && u.CognitoUserId != cognitoUserId)`. If true, log a warning and do not update username or slug. Return user unchanged. Ref: SPEC §7 Flow 3 step 2, SPEC §9 E-2.
- [x] 5.3 When the username change proceeds (no collision): update `user.Username = trimmedUsername`, regenerate slug using the collision resolution helper (task 3.1), update `user.Slug`, set `user.ModifiedAt = DateTime.UtcNow`. Ref: SPEC §7 Flow 3 steps 3-6.
- [x] 5.4 Log at info level when a slug is regenerated due to a username change. Ref: SPEC §8 Observability.

## 6. Service — Public Profile Lookup

- [x] 6.1 Change `IBbPointsService.GetPublicProfileDataAsync` parameter from `string username` to `string slug`. Ref: SPEC §6 Service Interface Changes.
- [x] 6.2 Update `BbPointsService.GetPublicProfileDataAsync` to query by slug: `_context.Users.FirstOrDefaultAsync(u => u.Slug == slug)` instead of `u.Username == username`. Ref: SPEC §7 Flow 4 step 3.
- [x] 6.3 Populate `CognitoUserId` on the returned `PublicProfileViewModel` from the found user record. Ref: SPEC §7 Flow 4 step 5.

## 7. Service — Submitter Points

- [x] 7.1 In `BbPointsService.GetSubmitterPointsAsync`, populate `Slug = user.Slug` on the returned `SubmitterPointsViewModel` when the user record exists. Ref: SPEC §7 Flow 5 step 4.
- [x] 7.2 In the fallback path (no user record), set `Slug = null` on the returned `SubmitterPointsViewModel`. Ref: SPEC §7 Flow 5 step 5.

## 8. ProfileController Changes

- [x] 8.1 Change the `Public` action route from `[Route("profile/{username}")]` to `[Route("profile/{slug}")]` and rename the parameter from `string username` to `string slug`. Ref: SPEC §6 Route Changes.
- [x] 8.2 Replace the self-redirect check: instead of comparing `User.GetPreferredUsername()` with the route parameter, call `GetPublicProfileDataAsync(slug)` first, then compare `model.CognitoUserId` with `User.GetCognitoUserId()`. If they match, redirect to `Index`. Ref: SPEC §7 Flow 4 steps 2-6.
- [x] 8.3 Update the log message in `Public` to reference slug instead of username. Ref: SPEC §7 Flow 4.

## 9. LyricController Changes

- [x] 9.1 Change the profile URL generation in `LyricController.Lyric()` from `$"/profile/{submitterPoints.Username}"` to `!string.IsNullOrEmpty(submitterPoints.Slug) ? $"/profile/{submitterPoints.Slug}" : null`. Ref: SPEC §6 Profile URL Generation, SPEC §7 Flow 5 step 6.

## 10. Retroactive Console App — Project Setup

- [x] 10.1 Add a `<ProjectReference Include="..\Bejebeje.Common\Bejebeje.Common.csproj" />` to `Bejebeje.Retroactive.csproj`. Ref: SPEC §10 Retroactive Console App — New Project Reference.
- [x] 10.2 Add `using Bejebeje.Common.Extensions;` to `Bejebeje.Retroactive/Program.cs` to access `NormalizeStringForUrl`. Ref: SPEC §4 Components Affected.

## 11. Retroactive Console App — Phase 1 Updates

- [x] 11.1 Trim the resolved Cognito username before storage in Phase 1: `username = username.Trim()` (or `preferredUsernameAttr?.Value?.Trim()`). Ref: SPEC §7 Flow 6 Phase 1 step 2b.
- [x] 11.2 Update the Phase 1 upsert SQL to include the `slug` column: set to NULL on insert, leave unchanged on conflict update (`slug = COALESCE(users.slug, NULL)` or simply exclude from the ON CONFLICT SET clause). Ref: SPEC §7 Flow 6 Phase 1 step 2d.

## 12. Retroactive Console App — Phase 2 (Slug Generation + Username Trimming Sweep)

- [x] 12.1 After Phase 1 completes, add Phase 2: query all users ordered by `id ASC` using `SELECT id, cognito_user_id, username, slug FROM users ORDER BY id ASC`. Ref: SPEC §7 Flow 6 Phase 2 step 1.
- [x] 12.2 For each user, implement username trimming: if `username != username.Trim()`, check if the trimmed value collides with another user's username via `SELECT COUNT(*) FROM users WHERE username = @trimmed AND id != @id`. If collision, skip the trim and log a warning. If no collision, update `username` to trimmed value. Ref: SPEC §7 Flow 6 Phase 2 step 2a.
- [x] 12.3 For each user, implement slug generation: apply `NormalizeStringForUrl()` to the (possibly trimmed) username. If empty, use `user-{cognitoUserId[..Min(8, len)]}`. Apply collision resolution by checking `SELECT COUNT(*) FROM users WHERE slug = @slug AND id != @id`, incrementing suffix until unique. Ref: SPEC §7 Flow 6 Phase 2 step 2b.
- [x] 12.4 Update each user's `slug` (and `username` if trimmed) in the database via `UPDATE users SET slug = @slug, username = @username WHERE id = @id`. Ref: SPEC §7 Flow 6 Phase 2 step 2c.
- [x] 12.5 Print Phase 2 summary: users processed, slugs generated, usernames trimmed, trim collisions skipped. Ref: SPEC §7 Flow 6 Phase 2 step 3.

## 13. Tests — Domain & Data Model

- [x] 13.1 Add a unit test verifying the `User` entity has a `Slug` property of type `string`. Ref: SPEC §5 User Entity Changes.

## 14. Tests — BbPointsService

- [x] 14.1 Add a test for `EnsureUserExistsAsync`: new user creation stores trimmed username and generated slug. Ref: SPEC §7 Flow 1.
- [x] 14.2 Add a test for `EnsureUserExistsAsync`: duplicate trimmed username on new user returns null without creating a record. Ref: SPEC §7 Flow 1 step 6, SPEC §9 E-1.
- [x] 14.3 Add a test for `EnsureUserExistsAsync`: returning user with trimmed Cognito username matching stored username triggers no update. Ref: SPEC §7 Flow 2 step 6.
- [x] 14.4 Add a test for `EnsureUserExistsAsync`: username change triggers slug regeneration and updates ModifiedAt. Ref: SPEC §7 Flow 3 steps 3-6.
- [x] 14.5 Add a test for `EnsureUserExistsAsync`: duplicate trimmed username on rename blocks the update. Ref: SPEC §7 Flow 3 step 2, SPEC §9 E-2.
- [x] 14.6 Add a test for `EnsureUserExistsAsync`: empty slug fallback is used when NormalizeStringForUrl returns empty. Ref: SPEC §7 Flow 7 steps 1-2, SPEC §9 E-3.
- [x] 14.7 Add a test for slug collision resolution: when a base slug exists for another user, the suffix `-2` is appended. Ref: SPEC §7 Flow 7 steps 3-6.
- [x] 14.8 Add a test for `GetPublicProfileDataAsync`: queries by slug and returns model with CognitoUserId populated. Ref: SPEC §7 Flow 4 steps 3-5.
- [x] 14.9 Add a test for `GetPublicProfileDataAsync`: returns null when no user matches the slug. Ref: SPEC §7 Flow 4 step 4.
- [x] 14.10 Add a test for `GetSubmitterPointsAsync`: returns Slug populated from user record when user exists. Ref: SPEC §7 Flow 5 step 4.
- [x] 14.11 Add a test for `GetSubmitterPointsAsync`: returns Slug as null in the fallback path (no local user record). Ref: SPEC §7 Flow 5 step 5.

## 15. Tests — Controllers

- [x] 15.1 Add a test for `ProfileController.Public`: authenticated user visiting own slug-based profile is redirected to Index. Ref: SPEC §7 Flow 4 step 6.
- [x] 15.2 Add a test for `ProfileController.Public`: authenticated user visiting another user's slug sees the public profile view. Ref: SPEC §7 Flow 4 step 6.
- [x] 15.3 Add a test for `ProfileController.Public`: non-existent slug returns 404. Ref: SPEC §7 Flow 4 step 4.
- [x] 15.4 Add a test for `LyricController.Lyric`: profile URL uses slug when available. Ref: SPEC §7 Flow 5 step 6.
- [x] 15.5 Add a test for `LyricController.Lyric`: profile URL is null when slug is null (fallback path). Ref: SPEC §7 Flow 5 step 6.
