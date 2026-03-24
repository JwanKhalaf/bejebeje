## 1. Domain Layer — Entities and Enums

- [x] 1.1 Create `ReportCategory` enum in `Bejebeje.Domain` with values: LyricsNotInKurdish (0), LyricsContainMistakes (1), Duplicate (2), WrongArtist (3), OffensiveOrInappropriate (4). Ref: SPEC §5 ReportCategory enum.
- [x] 1.2 Create `ReportStatus` enum in `Bejebeje.Domain` with values: Pending (0), Acknowledged (1), Dismissed (2). Ref: SPEC §5 ReportStatus enum.
- [x] 1.3 Create `LyricReport` entity in `Bejebeje.Domain` implementing `IBaseEntity` with properties: Id (int), LyricId (int), UserId (string), Category (int), Comment (string, nullable), Status (int, default 0), CreatedAt (DateTime), ModifiedAt (DateTime?, nullable), IsDeleted (bool, default false), ActionedBy (string, nullable), ActionedAt (DateTime?, nullable). Ref: SPEC §5 New Entity: LyricReport.

## 2. Data Access Layer — DbContext and Migration

- [x] 2.1 Register `DbSet<LyricReport>` in `BbContext`. Ref: SPEC §4 BbContext (extended).
- [x] 2.2 **[BLOCKED — user must create migration]** A database migration is required. Create `lyric_reports` table with snake_case columns: `id` (int, PK, auto-increment), `lyric_id` (int, FK to lyrics.id), `user_id` (string, not null), `category` (int, not null), `comment` (string, nullable), `status` (int, not null, default 0), `created_at` (timestamp, not null), `modified_at` (timestamp, nullable), `is_deleted` (bool, not null, default false), `actioned_by` (string, nullable), `actioned_at` (timestamp, nullable). Three indexes required: `ix_lyric_reports_user_id_created_at` on (user_id, created_at), `ix_lyric_reports_user_id_lyric_id_status` on (user_id, lyric_id, status), `ix_lyric_reports_lyric_id` on (lyric_id). Migration command: `dotnet-ef migrations add --project Bejebeje.DataAccess --startup-project Bejebeje.Mvc AddLyricReports`. Ref: SPEC §5 Database Table, §5 Indexes, §10 Database Migration.

## 3. Models Layer — ViewModels

- [x] 3.1 Create `LyricReportViewModel` in `Bejebeje.Models` with properties: LyricId (int), LyricTitle (string), LyricBody (string), ArtistName (string), ArtistSlug (string), LyricSlug (string), HasPendingReport (bool). Ref: SPEC §6 LyricReportViewModel.
- [x] 3.2 Create `LyricReportFormViewModel` in `Bejebeje.Models` with properties: LyricId (int, Required), Category (int, Required, Range(0,4)), Comment (string, Optional, MaxLength(2000)), ArtistSlug (string, Required), LyricSlug (string, Required). Apply data annotation validation attributes. Ref: SPEC §6 LyricReportFormViewModel.
- [x] 3.3 Create `LyricReportThankYouViewModel` in `Bejebeje.Models` with properties: LyricTitle (string), ArtistName (string), ArtistSlug (string), LyricSlug (string). Ref: SPEC §6 LyricReportThankYouViewModel.
- [x] 3.4 Create `LyricReportLimitReachedViewModel` in `Bejebeje.Models` with properties: LyricTitle (string), ArtistSlug (string), LyricSlug (string). Ref: SPEC §6 LyricReportLimitReachedViewModel.

## 4. Service Layer — ILyricReportsService

- [x] 4.1 Create `ILyricReportsService` interface in `Bejebeje.Services` (or `Bejebeje.Domain` per project conventions) with methods: `GetReportCountForUserTodayAsync(string userId)`, `HasPendingReportForLyricAsync(string userId, int lyricId)`, `GetLyricDetailsForReportAsync(string artistSlug, string lyricSlug)`, `CreateReportAsync(string userId, int lyricId, int category, string comment)`. Ref: SPEC §6 ILyricReportsService.
- [x] 4.2 Implement `LyricReportsService.GetReportCountForUserTodayAsync` using raw SQL via Npgsql. Query: count from `lyric_reports` WHERE `user_id = @userId AND created_at >= @utcTodayStart AND is_deleted = false`. Ref: SPEC §6 GetReportCountForUserTodayAsync, §8 Performance.
- [x] 4.3 Implement `LyricReportsService.HasPendingReportForLyricAsync` using raw SQL via Npgsql. Query: exists check WHERE `user_id = @userId AND lyric_id = @lyricId AND status = 0 AND is_deleted = false`. Ref: SPEC §6 HasPendingReportForLyricAsync, §8 Performance.
- [x] 4.4 Implement `LyricReportsService.GetLyricDetailsForReportAsync` using raw SQL via Npgsql. Fetch lyric title, body, artist name, artist slug, lyric slug, and lyric ID for an approved, non-deleted lyric. Return null if not found or not approved. Ref: SPEC §6 GetLyricDetailsForReportAsync.
- [x] 4.5 Implement `LyricReportsService.CreateReportAsync` using raw SQL via Npgsql. INSERT into `lyric_reports` with status = 0, created_at = DateTime.UtcNow. Return the new report ID. Ref: SPEC §6 CreateReportAsync.
- [x] 4.6 Implement email orchestration in `LyricReportsService` or `ReportController`: after saving the report, attempt admin notification and reporter confirmation emails wrapped in separate try/catch blocks. On email failure: log error (Sentry captures), continue flow. Report is persisted before emails are attempted. Ref: SPEC §7 Flow 3 steps 8-10, §8 Email Reliability.

## 5. Service Layer — IEmailService Extension

- [x] 5.1 Add `SendLyricReportNotificationEmailAsync(string reporterUsername, string lyricTitle, string artistName, string categoryDisplayLabel, string comment)` to `IEmailService` interface. Ref: SPEC §6 IEmailService extended.
- [x] 5.2 Add `SendLyricReportConfirmationEmailAsync(string reporterEmail, string lyricTitle, string artistName)` to `IEmailService` interface. Ref: SPEC §6 IEmailService extended.
- [x] 5.3 Implement `SendLyricReportNotificationEmailAsync` in `EmailService`. Send to admin (jk.bejebeje@gmail.com) via AWS SES with report details: reporter username, lyric title, artist name, category display label, comment. Follow existing SES send pattern (e.g. `SendArtistSubmissionEmailAsync`). Ref: SPEC §6 SendLyricReportNotificationEmailAsync.
- [x] 5.4 Implement `SendLyricReportConfirmationEmailAsync` in `EmailService`. Send to reporter's email via AWS SES with acknowledgement message including lyric title and artist name. Follow existing SES send pattern. Ref: SPEC §6 SendLyricReportConfirmationEmailAsync.

## 6. Controller — ReportController

- [x] 6.1 Create `ReportController` in `Bejebeje.Mvc/Controllers` decorated with `[Authorize]`. Inject `ILyricReportsService`, `IEmailService`, `ICognitoService`, and `ILogger`. Ref: SPEC §4 ReportController, §6 Authentication & Authorization.
- [x] 6.2 Implement `Report` GET action at route `GET /artists/{artistSlug}/lyrics/{lyricSlug}/report`. Fetch lyric details (redirect to Home if not found/not approved). Extract userId from `User.GetUserId().ToString()`. Check daily rate limit (redirect to limit-reached if >= 3). Check duplicate report (set HasPendingReport on view model). Return Report view. Ref: SPEC §7 Flow 2.
- [x] 6.3 Implement `SubmitReport` POST action at route `POST /artists/{artistSlug}/lyrics/{lyricSlug}/report` with `[ValidateAntiForgeryToken]`. Validate model state. Re-check rate limit and duplicate rules server-side. Re-fetch lyric details (redirect to Home if invalid). Call CreateReportAsync. Fetch reporter username via ICognitoService. Attempt admin notification email (try/catch). Read email claim, attempt reporter confirmation email if present (try/catch, log warning if null). Redirect to thank-you page with TempData. Ref: SPEC §7 Flow 3, §8 Security.
- [x] 6.4 Implement `ThankYou` GET action at route `GET /artists/{artistSlug}/lyrics/{lyricSlug}/report/thank-you`. Read lyric/artist info from TempData. If TempData empty, redirect to lyric detail page. Return ThankYou view. Ref: SPEC §7 Flow 4.
- [x] 6.5 Implement `LimitReached` GET action at route `GET /artists/{artistSlug}/lyrics/{lyricSlug}/report/limit-reached`. Load minimal lyric info for navigation. Return LimitReached view. Ref: SPEC §7 Flow 5.

## 7. Views — Razor Pages

- [x] 7.1 Create `Views/Report/Report.cshtml` — report form page. Display lyric title, artist name, full lyric text in a scrollable pane (max-height constrained). If HasPendingReport is true, show "You've already reported this lyric. We're reviewing it." with link back to lyric. Otherwise show form: category dropdown (required, 5 options), comment textarea (optional, maxlength 2000, character counter), hidden fields (LyricId, ArtistSlug, LyricSlug), anti-forgery token, submit button. Mobile-first Tailwind CSS layout. Ref: SPEC §7 Flow 2, Appendix A Page 2.
- [x] 7.2 Create `Views/Report/ThankYou.cshtml` — thank-you confirmation page. Display thank-you message. Two navigation links: "Go back to lyric" → `/artists/{artistSlug}/lyrics/{lyricSlug}`, "View other lyrics by [artist name]" → `/artists/{artistSlug}/lyrics`. Mobile-first Tailwind CSS. Ref: SPEC §7 Flow 4, Appendix A Page 4.
- [x] 7.3 Create `Views/Report/LimitReached.cshtml` — daily limit page. Display message: "You have reached your maximum of 3 reports for today. Please try again tomorrow." Navigation link: "Go back to lyric" → `/artists/{artistSlug}/lyrics/{lyricSlug}`. Mobile-first Tailwind CSS. Ref: SPEC §7 Flow 5, Appendix A Page 5.

## 8. View Modification — Lyric Detail Flag Icon

- [x] 8.1 Add flag SVG icon to `Views/Lyric/Lyric.cshtml` in the right-side column `<div class="flex flex-col gap-4">` container, below the like button. Wrap in `<a>` tag linking to `/artists/{Model.Artist.PrimarySlug}/lyrics/{Model.PrimarySlug}/report`. Only render when `Model.IsApproved` is true. Style with `size-10 lg:size-8` and `text-neutral-500` classes. Ref: SPEC §7 Flow 1, Appendix B.

## 9. Application Configuration — Program.cs

- [x] 9.1 Add `options.Scope.Add("email");` to the OIDC configuration in `Program.cs` (after existing `options.Scope.Add("openid");`). Ref: SPEC Appendix C, §10 Code Deployment.
- [x] 9.2 Register `builder.Services.AddScoped<ILyricReportsService, LyricReportsService>();` in `Program.cs`. Ref: SPEC Appendix C, §10 Code Deployment.

## 10. Client-Side Validation

- [x] 10.1 Add client-side validation on the report form: `required` attribute on category select, `maxlength="2000"` attribute on comment textarea. Optionally add a JavaScript character counter for the comment field. Ref: SPEC §8 Input Validation.

## 11. Edge Cases and Failure Handling

- [x] 11.1 Handle missing email claim: in the POST submission flow, read `User.FindFirstValue("email")`. If null, log a warning and skip the reporter confirmation email. Do not block the report. Ref: SPEC §9 "Reporter email claim is missing", §6 Authentication & Authorization.
- [x] 11.2 Handle Cognito username lookup failure: use the existing `ICognitoService.GetPreferredUsernameAsync` which falls back to "Unknown User" on failure. Ref: SPEC §9 "Cognito username lookup fails", §6 External Integrations.
- [x] 11.3 Handle lyric deleted/unapproved between GET and POST: in the POST handler, re-fetch lyric details. If not found or not approved, redirect to Home. Ref: SPEC §9 "Lyric is deleted or unapproved between loading report page and submitting".
- [x] 11.4 Handle double-submission via browser back button: server-side duplicate re-check on POST catches the second submission and redirects to report page showing "already reported". Ref: SPEC §9 "User submits report, then clicks back and submits again".

## 12. Testing

- [x] 12.1 Write unit tests for `LyricReportsService.GetReportCountForUserTodayAsync` — verify correct count for reports today, zero for reports from yesterday only, exclusion of deleted reports. Ref: SPEC §6 GetReportCountForUserTodayAsync.
- [x] 12.2 Write unit tests for `LyricReportsService.HasPendingReportForLyricAsync` — verify true when pending report exists, false when report is acknowledged/dismissed, false when no report exists. Ref: SPEC §6 HasPendingReportForLyricAsync.
- [x] 12.3 Write unit tests for `LyricReportsService.GetLyricDetailsForReportAsync` — verify correct data for approved lyric, null for unapproved, null for non-existent. Ref: SPEC §6 GetLyricDetailsForReportAsync.
- [x] 12.4 Write unit tests for `LyricReportsService.CreateReportAsync` — verify report is inserted with correct default values (status=0, created_at=UTC now). Ref: SPEC §6 CreateReportAsync.
- [x] 12.5 Write unit tests for `ReportController.Report` GET action — verify redirect to Home for invalid lyric, redirect to limit-reached when at daily limit, HasPendingReport set correctly, report form displayed for valid case. Ref: SPEC §7 Flow 2.
- [x] 12.6 Write unit tests for `ReportController.SubmitReport` POST action — verify validation failure returns form, server-side rate limit re-check works, server-side duplicate re-check works, report is saved and redirect to thank-you on success. Ref: SPEC §7 Flow 3.
- [x] 12.7 Write unit tests for `ReportController.ThankYou` GET action — verify redirect to lyric when TempData is empty, correct view when TempData is present. Ref: SPEC §7 Flow 4.
- [x] 12.8 Write unit tests for `ReportController.LimitReached` GET action — verify correct view model and navigation link. Ref: SPEC §7 Flow 5.
- [x] 12.9 Write unit tests for email error isolation — verify report is saved even when admin notification email throws, verify report is saved when reporter confirmation email throws. Ref: SPEC §8 Email Reliability, §9 failure modes.
- [x] 12.10 Write unit tests for `LyricReportFormViewModel` validation — verify Category is required and must be range 0-4, Comment max 2000 chars, LyricId and slugs required. Ref: SPEC §6 LyricReportFormViewModel, §8 Input Validation.
