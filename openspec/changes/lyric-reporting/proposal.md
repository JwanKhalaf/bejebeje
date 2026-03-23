## Why

There is no mechanism for users to flag problematic lyrics on Bejebeje (wrong language, mistakes, duplicates, wrong artist, offensive content), and admins have no structured way to receive, track, or action quality issues. This feature adds a reporting flow so logged-in users can submit reports that are stored in the shared database for the admin site to consume.

## What Changes

- Add a flag SVG icon to the lyric detail page linking to a new report page (visible to all users, only for approved lyrics)
- Add a new `ReportController` with GET/POST report form, thank-you page, and daily-limit-reached page
- Add a new `ILyricReportsService` / `LyricReportsService` with business logic for report submission, rate limiting (3/day per user), and duplicate prevention (one pending report per user per lyric)
- Add a new `LyricReport` domain entity implementing `IBaseEntity`, with `ReportCategory` and `ReportStatus` enums
- Add a new `lyric_reports` database table with indexes for efficient rate-limit and duplicate queries
- Extend `IEmailService` with two new methods: admin notification email and reporter confirmation email
- Add OIDC `email` scope to `Program.cs` configuration
- Register `ILyricReportsService` as scoped in `Program.cs`
- Add four new Razor views: Report page, Thank You page, Daily Limit Reached page, and Already Reported state
- Add three new ViewModels: `LyricReportViewModel`, `LyricReportFormViewModel`, `LyricReportThankYouViewModel`, `LyricReportLimitReachedViewModel`

## Capabilities

### New Capabilities
- `lyric-reporting`: Core reporting flow — data model, domain entities, enums, service layer, controller, report submission with server-side validation, rate limiting, duplicate prevention, report page, thank-you page, daily limit page, already-reported state
- `report-notifications`: Email notifications for lyric reports — admin notification email with report details, reporter confirmation email with acknowledgement, error isolation (email failures do not block report persistence)

### Modified Capabilities
- `lyric-detail`: Add flag SVG icon to the existing lyric detail page, linking to the report page. Only rendered for approved lyrics.

## Impact

- **Domain layer** (`Bejebeje.Domain`): New `LyricReport` entity, `ReportCategory` enum, `ReportStatus` enum
- **Data access** (`Bejebeje.DataAccess`): New `DbSet<LyricReport>` in `BbContext`; new database migration required for `lyric_reports` table with 3 indexes
- **Services** (`Bejebeje.Services`): New `ILyricReportsService` / `LyricReportsService`; extended `IEmailService` / `EmailService` with 2 new methods
- **Models** (`Bejebeje.Models`): 4 new ViewModels for reporting flow
- **MVC** (`Bejebeje.Mvc`): New `ReportController`; 4 new Razor views; modified `Lyric.cshtml` view; updated `Program.cs` (OIDC scope + service registration)
- **Database**: New `lyric_reports` table (shared with admin site)
- **External dependencies**: AWS SES (existing), AWS Cognito (existing, email scope already configured)

## References

- [REQUIREMENTS.md](../../REQUIREMENTS.md)
- [SPEC.md](../../SPEC.md)
