## ADDED Requirements

### Requirement: Admin notification email on new report submission
The system SHALL send an email to the admin address (jk.bejebeje@gmail.com) via AWS SES when a new report is submitted. The email SHALL include: reporter username, lyric title, artist name, report category display label, and comment (if provided). The method SHALL follow the existing EmailService pattern (throws on failure; caller handles error policy).

#### Scenario: Admin receives notification email for report with comment
- **GIVEN** a user submits a report with category "Lyrics contain mistakes" and a comment
- **WHEN** the admin notification email is sent
- **THEN** the admin receives an email at jk.bejebeje@gmail.com containing the reporter's username, lyric title, artist name, category "Lyrics contain mistakes", and the comment text

#### Scenario: Admin receives notification email for report without comment
- **GIVEN** a user submits a report with no comment
- **WHEN** the admin notification email is sent
- **THEN** the email is sent without a comment section (or with an indication that no comment was provided)

#### Scenario: Reporter username lookup fails
- **GIVEN** the Cognito username lookup fails for the reporter
- **WHEN** the admin notification email is composed
- **THEN** the reporter username defaults to "Unknown User" (existing CognitoService fallback behaviour)

### Requirement: Reporter confirmation email on new report submission
The system SHALL send a confirmation email to the reporter's email address (from the `email` OIDC claim) via AWS SES when a new report is submitted. The email SHALL acknowledge receipt and inform the reporter that their report will be reviewed.

#### Scenario: Reporter receives confirmation email
- **GIVEN** a user with a valid email claim submits a report for "Bo Min" by "Adnan Karim"
- **WHEN** the confirmation email is sent
- **THEN** the reporter receives an email acknowledging their report for "Bo Min" by "Adnan Karim" has been received and will be reviewed

#### Scenario: Reporter email claim is missing
- **GIVEN** a user whose session predates the email scope deployment (no email claim)
- **WHEN** a report is submitted
- **THEN** the reporter confirmation email is skipped, a warning is logged, and the report is still saved

### Requirement: Email failures do not block report persistence
The system SHALL persist the report to the database before attempting to send any emails. Email sending failures SHALL be caught, logged (captured by Sentry), and SHALL NOT prevent the report from being saved or the user from seeing the thank-you page.

#### Scenario: Admin notification email fails
- **GIVEN** the AWS SES call for the admin notification throws an exception
- **WHEN** the error is caught by LyricReportsService
- **THEN** the error is logged, the report remains saved in the database, and the user is redirected to the thank-you page

#### Scenario: Reporter confirmation email fails
- **GIVEN** the AWS SES call for the reporter confirmation throws an exception
- **WHEN** the error is caught by LyricReportsService
- **THEN** the error is logged, the report remains saved, and the user is redirected to the thank-you page

#### Scenario: Both emails fail
- **GIVEN** both the admin notification and reporter confirmation emails fail
- **WHEN** the errors are caught
- **THEN** both errors are logged independently, the report remains saved, and the user sees the thank-you page

### Requirement: IEmailService extended with two new methods
The system SHALL extend `IEmailService` with `SendLyricReportNotificationEmailAsync(string reporterUsername, string lyricTitle, string artistName, string categoryDisplayLabel, string comment)` and `SendLyricReportConfirmationEmailAsync(string reporterEmail, string lyricTitle, string artistName)`. Both methods SHALL follow the existing EmailService SES send pattern.

#### Scenario: SendLyricReportNotificationEmailAsync sends to admin
- **GIVEN** valid report details are provided
- **WHEN** SendLyricReportNotificationEmailAsync is called
- **THEN** an email is sent to jk.bejebeje@gmail.com via AWS SES with the provided report details

#### Scenario: SendLyricReportConfirmationEmailAsync sends to reporter
- **GIVEN** a valid reporter email and lyric/artist details
- **WHEN** SendLyricReportConfirmationEmailAsync is called
- **THEN** an email is sent to the reporter's address via AWS SES with an acknowledgement message
