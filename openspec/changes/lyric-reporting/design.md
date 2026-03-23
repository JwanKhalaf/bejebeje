## Context

Bejebeje is a community-driven Kurdish lyrics archive built on ASP.NET Core MVC with PostgreSQL, AWS Cognito for auth, and AWS SES for email. The codebase follows a clean architecture pattern (Domain → DataAccess → Services → Models → MVC) with raw SQL via Npgsql for data access, `IBaseEntity` for domain entities, and snake_case database naming.

Currently there is no way for users to flag problematic lyrics and no structured mechanism for admins to receive or track quality issues. The admin site (`bejebeje.admin`) shares the same PostgreSQL database but is a separate deployment — this feature only stores reports; the admin UI for actioning them is out of scope.

## Goals / Non-Goals

**Goals:**
- Add a complete vertical slice through all architecture layers for lyric reporting
- Follow all existing codebase conventions (raw SQL, IBaseEntity, snake_case, Tailwind CSS, scoped DI registration)
- Ensure the `lyric_reports` table is immediately queryable by the admin site after migration
- Isolate email failures from report persistence (report is always saved)

**Non-Goals:**
- Admin UI for reviewing/actioning reports (separate repo)
- BB Points integration
- Automatic actions on lyrics when reports are acknowledged
- Notifications to the reporter when their report is actioned
- Modifying or cancelling a report after submission

## Decisions

### 1. New `ReportController` rather than extending `LyricController`
**Decision:** Create a dedicated `ReportController` for all report-related actions.
**Rationale:** The reporting flow has its own routes, authorization requirements, and business logic. Keeping it separate follows the single responsibility principle and avoids bloating `LyricController`. The `LyricController` change is limited to a view-only modification (flag icon).
**Alternative considered:** Adding report actions to `LyricController` — rejected because it mixes concerns and the report flow has a distinct lifecycle.

### 2. Raw SQL via Npgsql for data access (not EF Core LINQ)
**Decision:** Use raw SQL with `NpgsqlConnection`/`NpgsqlCommand` for all `lyric_reports` queries, consistent with the existing codebase pattern.
**Rationale:** SPEC §4 and §3 (Constraint 6) mandate consistency with the existing data access pattern. `DbSet<LyricReport>` is registered in `BbContext` only for migration generation.
**Alternative considered:** EF Core LINQ queries — rejected for codebase consistency.

### 3. Report categories stored as integer-backed enum
**Decision:** Store `category` as an integer in the database, mapped to a C# `ReportCategory` enum.
**Rationale:** Integer storage is additive (new categories can be added without breaking existing records) and efficient for indexing. SPEC §5 defines the exact enum values.
**Alternative considered:** String storage — rejected because it's less efficient and more error-prone.

### 4. Email error isolation in the service layer (not controller)
**Decision:** `LyricReportsService` wraps each email call in try/catch, logs the error, and continues. The report is persisted before emails are attempted.
**Rationale:** SPEC §8 (Email Reliability) mandates that email failures never block report persistence. Putting error isolation in the service layer keeps the controller clean and ensures the policy is consistently applied.

### 5. TempData for thank-you page state
**Decision:** Pass lyric/artist info to the thank-you page via TempData on the redirect.
**Rationale:** SPEC §7 Flow 4 specifies this approach. If TempData is empty (direct navigation), redirect to lyric detail page. This avoids an extra database query on the thank-you page.

### 6. Anti-forgery token on POST
**Decision:** Use `[ValidateAntiForgeryToken]` on the report submission POST action.
**Rationale:** SPEC §8 (Security) mandates this. Consistent with ASP.NET Core MVC best practices for form submissions.

## Risks / Trade-offs

- **[Email delivery failure]** → Report is saved regardless. Admin misses notification for that specific report. Mitigated by Sentry error logging. Admin site (future) will surface all reports regardless of email status.
- **[Missing email claim for pre-deployment sessions]** → Reporter confirmation email is skipped, warning logged. Does not block report submission. Self-healing as users re-login.
- **[UTC calendar day for rate limiting]** → May confuse users in non-UTC timezones who expect a midnight reset at their local time. Accepted trade-off for simplicity per SPEC §3 Assumption 4.
- **[Reports accumulate before admin UI exists]** → Admins notified via email and can query database directly. Admin site work is a planned follow-on.
- **[Race condition on duplicate check]** → Two near-simultaneous submissions could both pass the duplicate check. Extremely unlikely in practice and the consequence is minor (two pending reports for the same lyric by the same user). A unique partial index could prevent this but adds complexity not justified by the risk.

## Migration Plan

1. **Database migration** must be applied before code deployment. Migration creates `lyric_reports` table with columns and 3 indexes as specified in SPEC §5.
2. **Cognito email scope** has already been deployed (2026-03-23 per SPEC §3 Assumption 1, §10). No further Cognito changes needed.
3. **Code deployment** includes all new files and modifications. No new environment variables or secrets required.
4. **Rollback**: Revert code deployment. Migration can be rolled back with `dotnet-ef migrations remove` if needed. No data loss concern since the table would be new/empty.

## Open Questions

- **Admin site timeline**: When will `bejebeje.admin` be updated to surface and action reports? Until then, admins rely on email notifications and direct database queries. (SPEC §11)
- **Email copy**: Exact wording of thank-you and admin notification emails to be determined during implementation. (SPEC §11)
