## Why

User profile pages return 404 when the Cognito `preferred_username` contains spaces or trailing whitespace. The profile URL route (`/profile/{username}`) uses the raw username as a URL path segment, but browsers normalise whitespace in URLs, causing the server-received value to differ from the stored value. 9 of 112 users have trailing whitespace, 13+ have internal spaces — none of these users have functional public profile pages via clicked links.

## What Changes

- Add a `Slug` property to the `User` domain entity and a corresponding `slug` column (text, not null, unique) to the `users` table via two database migrations (nullable first, then NOT NULL + unique index after data population).
- Implement slug generation using the existing `NormalizeStringForUrl` extension method in `Bejebeje.Common.Extensions.StringExtensions`, with collision resolution (`-2`, `-3`, etc.) and an empty-slug fallback (`user-{cognitoUserId[..8]}`).
- **BREAKING**: Change the public profile route from `/profile/{username}` to `/profile/{slug}`.
- Update `GetPublicProfileDataAsync` to query by `slug` instead of `username`.
- Update `ProfileController.Public` self-redirect check from `preferred_username` string comparison to `CognitoUserId` comparison using data returned from the service.
- Update `LyricController.Lyric` profile URL generation to use `submitterPoints.Slug` instead of `submitterPoints.Username`.
- Add `Slug` property to `SubmitterPointsViewModel` and `CognitoUserId` property to `PublicProfileViewModel`.
- Trim leading/trailing whitespace from usernames in `EnsureUserExistsAsync` on both creation and update paths, with duplicate username detection (FR-26, FR-27).
- Add a new Phase 2 to the Retroactive console app that sweeps all users to generate slugs and trim usernames (with collision-safe skip for the Kardox case).
- Add project reference from `Bejebeje.Retroactive` to `Bejebeje.Common` for access to `NormalizeStringForUrl`.
- Update EF Core configuration in `BbContext` to add unique index on `Slug`.

## Capabilities

### New Capabilities
- `user-slug-management`: Slug generation algorithm, collision resolution, empty-slug fallback, and User entity/schema changes.
- `data-population`: Retroactive console app Phase 2 — slug generation and username trimming sweep for all existing users.

### Modified Capabilities
- `profile-routing`: Profile URL generation on lyric detail page switches from raw username to slug; public profile lookup switches from username query to slug query; self-redirect uses CognitoUserId comparison.
- `user-record-sync`: EnsureUserExistsAsync gains username trimming, duplicate username detection (FR-26/FR-27), slug generation on create, and slug regeneration on username change.

## Impact

- **Bejebeje.Domain**: `User` entity — add `Slug` property.
- **Bejebeje.DataAccess**: `BbContext` — add unique index configuration on `User.Slug`. Two database migrations required (created manually by developer).
- **Bejebeje.Common**: No changes (existing `NormalizeStringForUrl` is reused as-is).
- **Bejebeje.Models**: `SubmitterPointsViewModel` — add `Slug`. `PublicProfileViewModel` — add `CognitoUserId`.
- **Bejebeje.Services**: `BbPointsService` — slug generation logic, trimming, duplicate checks, lookup changes. `IBbPointsService` — `GetPublicProfileDataAsync` parameter semantics change from username to slug.
- **Bejebeje.Mvc**: `ProfileController` — route parameter + self-redirect logic. `LyricController` — profile URL generation.
- **Bejebeje.Retroactive**: Add project reference to `Bejebeje.Common`; add Phase 2 slug generation + username trimming sweep.
- **Bejebeje.Mvc.Tests / Bejebeje.Services.Tests**: Update tests for all changed components.
