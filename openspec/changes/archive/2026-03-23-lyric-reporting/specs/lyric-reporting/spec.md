## ADDED Requirements

### Requirement: LyricReport domain entity with IBaseEntity and audit fields
The system SHALL provide a `LyricReport` entity implementing `IBaseEntity` with fields: Id (int, PK), LyricId (int, FK), UserId (string), Category (int, enum-backed), Comment (string, nullable, max 2000), Status (int, enum-backed, default 0), CreatedAt, ModifiedAt, IsDeleted, ActionedBy (string, nullable), ActionedAt (DateTime, nullable). Reports SHALL never be hard-deleted.

#### Scenario: LyricReport entity is created with correct default values
- **GIVEN** a user submits a new lyric report
- **WHEN** the report is persisted to the database
- **THEN** the report has Status = 0 (Pending), CreatedAt = current UTC time, IsDeleted = false, ModifiedAt = null, ActionedBy = null, ActionedAt = null

#### Scenario: LyricReport entity enforces comment max length
- **GIVEN** a user submits a report with a comment exceeding 2000 characters
- **WHEN** server-side validation runs
- **THEN** the submission is rejected with a validation error

### Requirement: ReportCategory enum with five predefined values
The system SHALL define a `ReportCategory` enum with values: LyricsNotInKurdish (0), LyricsContainMistakes (1), Duplicate (2), WrongArtist (3), OffensiveOrInappropriate (4).

#### Scenario: All five report categories are available for selection
- **GIVEN** a user is on the report form page
- **WHEN** the category dropdown is rendered
- **THEN** all five categories are available with their display labels: "Lyrics are not in Kurdish", "Lyrics contain mistakes", "Duplicate", "Wrong artist", "Offensive or inappropriate content"

### Requirement: ReportStatus enum for report lifecycle
The system SHALL define a `ReportStatus` enum with values: Pending (0), Acknowledged (1), Dismissed (2).

#### Scenario: New report is created with Pending status
- **GIVEN** a user submits a new report
- **WHEN** the report is saved
- **THEN** the status is set to Pending (0)

### Requirement: lyric_reports database table with indexes
The system SHALL create a `lyric_reports` table with snake_case columns and three indexes: `ix_lyric_reports_user_id_created_at` on (user_id, created_at), `ix_lyric_reports_user_id_lyric_id_status` on (user_id, lyric_id, status), and `ix_lyric_reports_lyric_id` on (lyric_id).

#### Scenario: Daily rate limit query uses the user_id/created_at index
- **GIVEN** the lyric_reports table has the ix_lyric_reports_user_id_created_at index
- **WHEN** a rate limit count query filters on user_id and created_at >= today's UTC start
- **THEN** the query uses the index and returns a scalar count without a full table scan

#### Scenario: Duplicate report check uses the user_id/lyric_id/status index
- **GIVEN** the lyric_reports table has the ix_lyric_reports_user_id_lyric_id_status index
- **WHEN** a duplicate check query filters on user_id, lyric_id, and status = 0
- **THEN** the query uses the index and returns a boolean result without a full table scan

### Requirement: DbSet registration for migration generation
The system SHALL register `DbSet<LyricReport>` in `BbContext` to enable EF Core migration generation. Runtime queries SHALL use raw SQL via Npgsql, not EF Core LINQ.

#### Scenario: EF Core migration can be generated for lyric_reports table
- **GIVEN** LyricReport is registered as a DbSet in BbContext
- **WHEN** a new EF Core migration is created
- **THEN** the migration generates the lyric_reports table with correct columns, types, and snake_case naming

### Requirement: ILyricReportsService with rate limit checking
The system SHALL provide `ILyricReportsService.GetReportCountForUserTodayAsync(string userId)` that counts non-deleted reports for the given user where created_at >= current UTC date start.

#### Scenario: User has submitted 2 reports today
- **GIVEN** a user has 2 non-deleted reports with created_at on the current UTC date
- **WHEN** GetReportCountForUserTodayAsync is called with that user's ID
- **THEN** the method returns 2

#### Scenario: User has reports from yesterday only
- **GIVEN** a user has 3 reports from yesterday (UTC) and none today
- **WHEN** GetReportCountForUserTodayAsync is called with that user's ID
- **THEN** the method returns 0

### Requirement: ILyricReportsService with duplicate report checking
The system SHALL provide `ILyricReportsService.HasPendingReportForLyricAsync(string userId, int lyricId)` that checks for a non-deleted report with status = Pending for the given user and lyric.

#### Scenario: User has a pending report for the lyric
- **GIVEN** a user has a report with status = Pending for lyric ID 42
- **WHEN** HasPendingReportForLyricAsync is called with that user ID and lyric ID 42
- **THEN** the method returns true

#### Scenario: User has an acknowledged report for the lyric but no pending one
- **GIVEN** a user has a report with status = Acknowledged for lyric ID 42 and no pending report
- **WHEN** HasPendingReportForLyricAsync is called with that user ID and lyric ID 42
- **THEN** the method returns false (user can re-report)

### Requirement: ILyricReportsService with lyric details fetching for report page
The system SHALL provide `ILyricReportsService.GetLyricDetailsForReportAsync(string artistSlug, string lyricSlug)` that fetches lyric title, body, artist name, artist slug, lyric slug, and lyric ID for an approved, non-deleted lyric. Returns null if not found or not approved.

#### Scenario: Approved lyric exists
- **GIVEN** an approved, non-deleted lyric exists with artist slug "adnan-karim" and lyric slug "bo-min"
- **WHEN** GetLyricDetailsForReportAsync is called with those slugs
- **THEN** a LyricReportViewModel is returned with correct lyric title, body, artist name, and slugs

#### Scenario: Lyric is not approved
- **GIVEN** a lyric exists but is not approved
- **WHEN** GetLyricDetailsForReportAsync is called with that lyric's slugs
- **THEN** null is returned

#### Scenario: Lyric does not exist
- **GIVEN** no lyric exists for the given artist slug and lyric slug
- **WHEN** GetLyricDetailsForReportAsync is called
- **THEN** null is returned

### Requirement: ILyricReportsService with report creation
The system SHALL provide `ILyricReportsService.CreateReportAsync(string userId, int lyricId, int category, string comment)` that inserts a new report with status = 0 and created_at = DateTime.UtcNow, returning the new report ID.

#### Scenario: Report is successfully created
- **GIVEN** a valid user ID, lyric ID, category, and optional comment
- **WHEN** CreateReportAsync is called
- **THEN** a new row is inserted into lyric_reports with status = 0, created_at = UTC now, and the new report ID is returned

### Requirement: ReportController with authorized GET report page
The system SHALL provide `ReportController.Report` action at `GET /artists/{artistSlug}/lyrics/{lyricSlug}/report` decorated with `[Authorize]`. The action SHALL check rate limit, duplicate status, and return the report view or redirect appropriately.

#### Scenario: Authenticated user accesses report page for valid lyric
- **GIVEN** an authenticated user who has not hit the daily limit and has no pending report
- **WHEN** they navigate to GET /artists/{artistSlug}/lyrics/{lyricSlug}/report
- **THEN** the report form is displayed with lyric title, artist name, full lyric text, category dropdown, and comment field

#### Scenario: Unauthenticated user accesses report page
- **GIVEN** an unauthenticated user
- **WHEN** they navigate to GET /artists/{artistSlug}/lyrics/{lyricSlug}/report
- **THEN** they are redirected to the Cognito login page and returned to the report URL after authentication

#### Scenario: User has reached daily limit
- **GIVEN** an authenticated user who has submitted 3 reports today
- **WHEN** they navigate to GET /artists/{artistSlug}/lyrics/{lyricSlug}/report
- **THEN** they are redirected to the daily limit reached page

#### Scenario: User has a pending report for this lyric
- **GIVEN** an authenticated user with a pending report for this specific lyric
- **WHEN** they navigate to GET /artists/{artistSlug}/lyrics/{lyricSlug}/report
- **THEN** the page shows "You've already reported this lyric. We're reviewing it." instead of the form

#### Scenario: Lyric is not found or not approved
- **GIVEN** the requested lyric does not exist or is not approved
- **WHEN** a user navigates to GET /artists/{artistSlug}/lyrics/{lyricSlug}/report
- **THEN** the user is redirected to Home/Index

### Requirement: ReportController with authorized POST report submission
The system SHALL provide `ReportController.SubmitReport` action at `POST /artists/{artistSlug}/lyrics/{lyricSlug}/report` decorated with `[Authorize]` and `[ValidateAntiForgeryToken]`. The action SHALL re-validate rate limit and duplicate rules server-side before saving.

#### Scenario: Valid report submission
- **GIVEN** an authenticated user submits a report with valid category and optional comment
- **WHEN** the POST is processed and all server-side checks pass
- **THEN** the report is saved, emails are attempted, and the user is redirected to the thank-you page

#### Scenario: Server-side rate limit re-check rejects submission
- **GIVEN** a user has reached 3 reports today (possibly via another tab)
- **WHEN** they POST a report submission
- **THEN** the server-side rate limit check fails and the user is redirected to the limit-reached page

#### Scenario: Server-side duplicate re-check rejects submission
- **GIVEN** a user already has a pending report for this lyric (possibly submitted in another tab)
- **WHEN** they POST a report submission for the same lyric
- **THEN** the server-side duplicate check fails and the user is redirected back to the report page (GET)

#### Scenario: Invalid model state (missing category or oversized comment)
- **GIVEN** the submitted form has an invalid category or comment exceeding 2000 characters
- **WHEN** the POST is processed
- **THEN** model validation fails and the report form is re-displayed with validation errors

#### Scenario: Anti-forgery token is missing or invalid
- **GIVEN** a POST request without a valid anti-forgery token
- **WHEN** the request hits the SubmitReport action
- **THEN** the request is rejected by ValidateAntiForgeryToken

#### Scenario: Lyric is deleted or unapproved between page load and submission
- **GIVEN** a lyric was valid when the user loaded the form but is now deleted/unapproved
- **WHEN** the user submits the report
- **THEN** the POST handler re-fetches lyric details and redirects to Home if not found/not approved

### Requirement: Thank-you page after successful report submission
The system SHALL provide `ReportController.ThankYou` at `GET /artists/{artistSlug}/lyrics/{lyricSlug}/report/thank-you` that displays a confirmation message with navigation links. Lyric/artist info is read from TempData.

#### Scenario: User is redirected after successful submission
- **GIVEN** a user has just submitted a report successfully
- **WHEN** they are redirected to the thank-you page
- **THEN** the page shows a thank-you message, a "Go back to lyric" link, and a "View other lyrics by [artist name]" link

#### Scenario: Direct navigation to thank-you page without submission
- **GIVEN** a user navigates directly to the thank-you URL without TempData
- **WHEN** the page loads
- **THEN** the user is redirected to the lyric detail page

### Requirement: Daily limit reached page
The system SHALL provide `ReportController.LimitReached` at `GET /artists/{artistSlug}/lyrics/{lyricSlug}/report/limit-reached` that informs the user of the 3/day limit with a link back to the lyric.

#### Scenario: User sees the daily limit page
- **GIVEN** a user has been redirected to the limit-reached page
- **WHEN** the page renders
- **THEN** it shows "You have reached your maximum of 3 reports for today. Please try again tomorrow." and a "Go back to lyric" link

### Requirement: ViewModels for reporting flow
The system SHALL provide four ViewModels: `LyricReportViewModel` (report page GET), `LyricReportFormViewModel` (report page POST with validation attributes), `LyricReportThankYouViewModel` (thank-you page), and `LyricReportLimitReachedViewModel` (limit-reached page).

#### Scenario: LyricReportFormViewModel enforces validation rules
- **GIVEN** a LyricReportFormViewModel instance
- **WHEN** Category is missing or out of range 0-4, or Comment exceeds 2000 characters
- **THEN** model validation fails with appropriate error messages

### Requirement: Report page displays lyric context and mobile-first form
The system SHALL render a report page with the lyric title, artist name, and full lyric text in a scrollable pane (max-height constrained), plus a form with a required category dropdown and optional comment textarea (max 2000 chars). The layout SHALL be mobile-first using Tailwind CSS, consistent with existing site styling.

#### Scenario: Report page renders correctly on mobile
- **GIVEN** a user accesses the report page on a mobile device
- **WHEN** the page renders
- **THEN** the lyric text pane is scrollable with a max-height, the form inputs are full-width, and the layout is usable on small screens

#### Scenario: Report page renders correctly on desktop
- **GIVEN** a user accesses the report page on a desktop browser
- **WHEN** the page renders
- **THEN** the layout adapts appropriately and all elements are accessible

### Requirement: User ID is always read from server-side claims
The system SHALL always read the user ID from the `sub` claim via `User.GetUserId().ToString()` on the server side. The user ID SHALL never be accepted from form input.

#### Scenario: User ID cannot be spoofed via form data
- **GIVEN** a malicious user submits a POST with a different user ID in the form body
- **WHEN** the server processes the request
- **THEN** the server ignores the form user ID and uses the authenticated user's sub claim

### Requirement: Service registration and OIDC configuration
The system SHALL register `ILyricReportsService` / `LyricReportsService` as scoped in `Program.cs` and add the `email` scope to the OIDC configuration.

#### Scenario: LyricReportsService is resolved via dependency injection
- **GIVEN** the application has started with the updated Program.cs
- **WHEN** a controller requests ILyricReportsService via constructor injection
- **THEN** a LyricReportsService instance is provided

#### Scenario: Email claim is available after OIDC scope is added
- **GIVEN** the OIDC configuration includes the email scope
- **WHEN** a user logs in via Cognito
- **THEN** the email claim is available on the user's ClaimsPrincipal
