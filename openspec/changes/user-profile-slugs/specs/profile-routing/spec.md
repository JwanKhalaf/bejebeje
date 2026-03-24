## MODIFIED Requirements

### Requirement: Public profile route SHALL use slug instead of username
The public profile route SHALL change from `[Route("profile/{username}")]` to `[Route("profile/{slug}")]`. The `ProfileController.Public` action parameter SHALL be `string slug`.

#### Scenario: Public profile accessible via slug-based URL
- **GIVEN** a user with slug `"ali-fm"`
- **WHEN** a visitor navigates to `/profile/ali-fm`
- **THEN** the public profile page SHALL be returned with the user's data

#### Scenario: Public profile returns 404 for non-existent slug
- **GIVEN** no user exists with slug `"nonexistent"`
- **WHEN** a visitor navigates to `/profile/nonexistent`
- **THEN** the response SHALL be 404

### Requirement: Public profile lookup SHALL query by slug instead of username
`GetPublicProfileDataAsync` SHALL accept a `slug` parameter and query `_context.Users.FirstOrDefaultAsync(u => u.Slug == slug)` instead of querying by `Username`.

#### Scenario: Service queries users table by slug column
- **GIVEN** a user with slug `"ali-fm"` and username `"ali fm"`
- **WHEN** `GetPublicProfileDataAsync("ali-fm")` is called
- **THEN** the service SHALL query `WHERE slug = 'ali-fm'` and return the matching user's profile data

#### Scenario: Service returns null for non-existent slug
- **GIVEN** no user has slug `"missing"`
- **WHEN** `GetPublicProfileDataAsync("missing")` is called
- **THEN** the service SHALL return null

### Requirement: IBbPointsService.GetPublicProfileDataAsync signature SHALL change to accept slug
The method signature on `IBbPointsService` SHALL change from `Task<PublicProfileViewModel> GetPublicProfileDataAsync(string username)` to `Task<PublicProfileViewModel> GetPublicProfileDataAsync(string slug)`.

#### Scenario: Interface method parameter represents slug
- **GIVEN** the `IBbPointsService` interface
- **WHEN** the `GetPublicProfileDataAsync` method signature is inspected
- **THEN** the parameter SHALL be named `slug` and represent a URL-safe slug value

### Requirement: Self-redirect SHALL compare CognitoUserId instead of username string
When a logged-in user navigates to `/profile/{slug}`, the controller SHALL determine whether the slug belongs to the authenticated user by comparing `model.CognitoUserId` with `User.GetCognitoUserId()`. If they match, redirect to `/profile` (own profile view).

#### Scenario: Authenticated user visiting own public profile URL is redirected
- **GIVEN** an authenticated user with CognitoUserId `"abc-123"` AND slug `"ali-fm"`
- **WHEN** they navigate to `/profile/ali-fm`
- **THEN** `GetPublicProfileDataAsync` SHALL return a model with `CognitoUserId = "abc-123"` AND the controller SHALL redirect to `/profile`

#### Scenario: Authenticated user visiting another user's profile sees the public page
- **GIVEN** an authenticated user with CognitoUserId `"abc-123"` AND another user has slug `"other-user"` with CognitoUserId `"def-456"`
- **WHEN** the authenticated user navigates to `/profile/other-user`
- **THEN** the public profile view SHALL be returned (no redirect)

#### Scenario: Anonymous user visiting a profile sees the public page
- **GIVEN** an anonymous (unauthenticated) visitor
- **WHEN** they navigate to `/profile/ali-fm`
- **THEN** the public profile view SHALL be returned (no redirect check)

### Requirement: PublicProfileViewModel SHALL include CognitoUserId property
`PublicProfileViewModel` SHALL have a `CognitoUserId` property of type `string`. It is used by the controller for self-redirect logic and is NOT rendered in the view.

#### Scenario: PublicProfileViewModel includes CognitoUserId
- **GIVEN** `GetPublicProfileDataAsync` returns a `PublicProfileViewModel`
- **WHEN** the model is inspected
- **THEN** `CognitoUserId` SHALL be populated with the user's Cognito user ID

### Requirement: Profile URL generation on lyric detail page SHALL use slug
`LyricController.Lyric()` SHALL set `viewModel.SubmitterProfileUrl` to `/profile/{submitterPoints.Slug}` when the slug is non-null/non-empty, or `null` when the slug is null (fallback path — no local user record).

#### Scenario: Lyric detail page generates slug-based profile URL
- **GIVEN** a lyric submitted by a user with slug `"ali-fm"` and username `"ali fm"`
- **WHEN** the lyric detail page is rendered
- **THEN** `SubmitterProfileUrl` SHALL be `"/profile/ali-fm"` and the display name SHALL remain `"ali fm"`

#### Scenario: Lyric detail page has no profile link when submitter has no local user record
- **GIVEN** a lyric submitted by a Cognito user who has no local user record (slug is null)
- **WHEN** the lyric detail page is rendered
- **THEN** `SubmitterProfileUrl` SHALL be null and the username SHALL be rendered as plain text without a link

### Requirement: SubmitterPointsViewModel SHALL include Slug property
`SubmitterPointsViewModel` SHALL have a `Slug` property of type `string`. It SHALL be null when the user has no local user record (Cognito-only fallback).

#### Scenario: SubmitterPointsViewModel includes slug for user with local record
- **GIVEN** a user with a local user record and slug `"ali-fm"`
- **WHEN** `GetSubmitterPointsAsync` returns the view model
- **THEN** `Slug` SHALL be `"ali-fm"`

#### Scenario: SubmitterPointsViewModel has null slug for fallback user
- **GIVEN** a Cognito user with no local user record
- **WHEN** `GetSubmitterPointsAsync` falls back to resolving username from Cognito API
- **THEN** `Slug` SHALL be null

### Requirement: Display name on profile and lyric pages SHALL remain the stored username
The public profile page SHALL display `Model.Username` (the stored, trimmed username) as the display name. The lyric detail page SHALL display `SubmitterUsername` as the display name. Neither page SHALL display the slug as a display name.

#### Scenario: Public profile page shows username not slug
- **GIVEN** a user with username `"ali fm"` and slug `"ali-fm"`
- **WHEN** the public profile page is viewed at `/profile/ali-fm`
- **THEN** the displayed name SHALL be `"ali fm"`, not `"ali-fm"`
