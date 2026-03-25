## ADDED Requirements

### Requirement: Login page with email and password authentication
The system SHALL provide a login page at `/login` where users can authenticate with their email and password. The page SHALL include a "Remember me" checkbox. The system SHALL authenticate against Cognito using the `USER_PASSWORD_AUTH` flow via `IAuthService.AuthenticateAsync`. On success, the system SHALL create a `ClaimsIdentity` with temporary token claims and call `SignInAsync`. On failure, the system SHALL display user-friendly error messages without revealing whether the email exists.

#### Scenario: Successful login without remember me
- **GIVEN** a registered and confirmed user exists in Cognito
- **WHEN** the user submits valid email and password with "Remember me" unchecked
- **THEN** the system authenticates via Cognito, issues a session cookie (non-persistent), and redirects to `/`

#### Scenario: Successful login with remember me
- **GIVEN** a registered and confirmed user exists in Cognito
- **WHEN** the user submits valid email and password with "Remember me" checked
- **THEN** the system issues a persistent cookie with 30-day expiry and redirects to `/`

#### Scenario: Successful login with ReturnUrl
- **GIVEN** the user was redirected to `/login?ReturnUrl=/profile`
- **WHEN** the user submits valid credentials
- **THEN** the system redirects to `/profile` (the local ReturnUrl)

#### Scenario: Login with external ReturnUrl is rejected
- **GIVEN** the user navigates to `/login?ReturnUrl=https://evil.com`
- **WHEN** the user submits valid credentials
- **THEN** the system redirects to `/` (not the external URL)

#### Scenario: Login with invalid credentials
- **GIVEN** the user enters an incorrect email or password
- **WHEN** the form is submitted
- **THEN** the system re-renders the login page with "Invalid email or password. Please try again." and the email field retains its value

#### Scenario: Login with unconfirmed email
- **GIVEN** the user signed up but has not confirmed their email
- **WHEN** the user attempts to log in
- **THEN** the system displays "Please confirm your email address before logging in."

#### Scenario: Login with too many Cognito attempts
- **GIVEN** Cognito returns `TooManyRequestsException`
- **WHEN** the user attempts to log in
- **THEN** the system displays "Too many failed attempts. Please try again later."

#### Scenario: Login CSRF protection
- **GIVEN** a login form submission
- **WHEN** the anti-forgery token is missing or invalid
- **THEN** the request is rejected

### Requirement: Sign-up page with email, username, and password
The system SHALL provide a sign-up page at `/signup` where users can create an account with email, username (preferred_username), and password. The page SHALL include client-side password validation matching the Cognito policy (14+ chars, 1 number, 1 uppercase, 1 lowercase). On success, the system SHALL set `TempData["SignUpEmail"]` and redirect to `/signup/confirm`. On failure, the system SHALL display user-friendly error messages.

#### Scenario: Successful sign-up
- **GIVEN** the user enters a valid email, username, and compliant password
- **WHEN** the form is submitted
- **THEN** the system calls Cognito `SignUp`, sets `TempData["SignUpEmail"]`, and redirects to `/signup/confirm`

#### Scenario: Sign-up with existing email
- **GIVEN** an account with the email already exists in Cognito
- **WHEN** the user attempts to sign up
- **THEN** the system displays "An account with this email already exists."

#### Scenario: Sign-up with invalid password
- **GIVEN** the user enters a password that does not meet Cognito policy
- **WHEN** the form is submitted
- **THEN** the system displays "Password does not meet the required criteria."

#### Scenario: Client-side password validation
- **GIVEN** the user is on the sign-up page
- **WHEN** the user types in the password field
- **THEN** a live checklist shows which criteria are met (14+ chars, 1 number, 1 uppercase, 1 lowercase) and the submit button is disabled until all are met

#### Scenario: Sign-up CSRF protection
- **GIVEN** a sign-up form submission
- **WHEN** the anti-forgery token is missing or invalid
- **THEN** the request is rejected

### Requirement: Email confirmation page for post-sign-up verification
The system SHALL provide an email confirmation page at `/signup/confirm` that is only reachable via redirect from a successful sign-up (guarded by `TempData["SignUpEmail"]`). The page SHALL include a pre-populated email field (hidden) and a confirmation code field. On success, the system SHALL create the local user record via `GetUserByEmailAsync` + `EnsureUserExistsAsync` and redirect to `/login`.

#### Scenario: Successful email confirmation
- **GIVEN** the user was redirected from sign-up with a valid TempData email
- **WHEN** the user enters the correct confirmation code
- **THEN** the system confirms with Cognito, creates the local user record, and redirects to `/login`

#### Scenario: Confirmation with invalid code
- **GIVEN** the user enters an incorrect confirmation code
- **WHEN** the form is submitted
- **THEN** the system displays "Invalid confirmation code. Please check the code and try again."

#### Scenario: Confirmation with expired code
- **GIVEN** the confirmation code has expired
- **WHEN** the form is submitted
- **THEN** the system displays "The confirmation code has expired. Please request a new one."

#### Scenario: Direct navigation to confirm page
- **GIVEN** the user navigates directly to `/signup/confirm` (no TempData)
- **WHEN** the page loads
- **THEN** the user is redirected to `/signup`

#### Scenario: Page refresh on confirm page
- **GIVEN** the user is on `/signup/confirm` and refreshes the page
- **WHEN** the page reloads (TempData consumed)
- **THEN** the user is redirected to `/signup`

#### Scenario: Local user creation fails during confirmation
- **GIVEN** `GetUserByEmailAsync` or `EnsureUserExistsAsync` fails
- **WHEN** the email is confirmed successfully
- **THEN** the error is logged, the user is still redirected to `/login`, and the `OnSigningIn` safety net will retry on first login

#### Scenario: Duplicate username collision during confirmation
- **GIVEN** `EnsureUserExistsAsync` returns `null` due to duplicate local username
- **WHEN** the email is confirmed successfully
- **THEN** a warning is logged, the user is redirected to `/login`, and can still log in (BB Points handles missing user records gracefully)

### Requirement: Resend confirmation code functionality
The confirmation page SHALL include a "Resend code" button that calls Cognito `ResendConfirmationCode`. Regardless of outcome, the page SHALL display "A new code has been sent to your email." and remain on the same page.

#### Scenario: Resend confirmation code
- **GIVEN** the user is on the confirmation page
- **WHEN** the user clicks "Resend code"
- **THEN** the system calls Cognito to resend the code and displays a success message, remaining on the same page with the email still populated

### Requirement: Forgotten password page to request reset code
The system SHALL provide a forgotten password page at `/forgotten-password` where users enter their email to receive a reset code. The response SHALL be identical whether the email exists or not (no information disclosure).

#### Scenario: Forgotten password request
- **GIVEN** the user enters an email address
- **WHEN** the form is submitted
- **THEN** the system calls Cognito `ForgotPassword` and redirects to `/reset-password?email={email}` regardless of whether the email exists

#### Scenario: Forgotten password for non-existent email
- **GIVEN** the user enters an email that does not exist in Cognito
- **WHEN** the form is submitted
- **THEN** the system still redirects to `/reset-password?email={email}` (identical behavior to existing email)

### Requirement: Reset password page to set a new password
The system SHALL provide a reset password page at `/reset-password` with email (pre-populated from query string), confirmation code, new password, and confirm password fields. Client-side validation SHALL enforce the Cognito password policy and password match. On success, the system SHALL redirect to `/login`.

#### Scenario: Successful password reset
- **GIVEN** the user enters a valid code and a compliant new password
- **WHEN** the form is submitted
- **THEN** the system confirms with Cognito and redirects to `/login`

#### Scenario: Reset with invalid code
- **GIVEN** the user enters an incorrect confirmation code
- **WHEN** the form is submitted
- **THEN** the system displays "Invalid confirmation code. Please check the code and try again."

#### Scenario: Reset with expired code
- **GIVEN** the confirmation code has expired
- **WHEN** the form is submitted
- **THEN** the system displays "The confirmation code has expired. Please request a new one."

#### Scenario: Reset with non-compliant password
- **GIVEN** the user enters a password that does not meet the Cognito policy
- **WHEN** the form is submitted
- **THEN** the system displays "Password does not meet the required criteria."

#### Scenario: Client-side validation on reset page
- **GIVEN** the user is on the reset password page
- **WHEN** the user types in the password fields
- **THEN** a live checklist shows password criteria status and password match status, with submit disabled until all criteria are met

### Requirement: Logout endpoint
The system SHALL provide a logout endpoint at `/logout` that clears the authentication cookie and redirects to `/login`. The endpoint SHALL NOT require authentication.

#### Scenario: Authenticated user logs out
- **GIVEN** an authenticated user
- **WHEN** the user navigates to `/logout`
- **THEN** the authentication cookie is cleared and the user is redirected to `/login`

#### Scenario: Unauthenticated user hits logout
- **GIVEN** a user with an expired or no session
- **WHEN** the user navigates to `/logout`
- **THEN** the system clears any cookie and redirects to `/login` without error

### Requirement: View models for all auth pages
The system SHALL have view models in `Bejebeje.Models/Account/` for each auth page: `LoginViewModel`, `SignupViewModel`, `ConfirmViewModel`, `ForgottenPasswordViewModel`, `ResetPasswordViewModel`. Each SHALL contain the form fields and error/success message properties as defined in SPEC.md §6.4.

#### Scenario: View models contain required properties
- **GIVEN** the auth page view models
- **WHEN** inspected
- **THEN** `LoginViewModel` has `Email`, `Password`, `RememberMe`, `ReturnUrl`, `ErrorMessage`; `SignupViewModel` has `Email`, `Username`, `Password`, `ErrorMessage`; `ConfirmViewModel` has `Email`, `Code`, `ErrorMessage`, `SuccessMessage`; `ForgottenPasswordViewModel` has `Email`, `ErrorMessage`; `ResetPasswordViewModel` has `Email`, `Code`, `NewPassword`, `ConfirmPassword`, `ErrorMessage`

### Requirement: Anti-forgery protection on all auth forms
All authentication form submissions SHALL include `@Html.AntiForgeryToken()` in the view and `[ValidateAntiForgeryToken]` on the POST action.

#### Scenario: All POST actions validate anti-forgery tokens
- **GIVEN** any auth page POST endpoint
- **WHEN** a request is made without a valid anti-forgery token
- **THEN** the request is rejected

### Requirement: All auth pages accessible without authentication
All authentication pages (login, sign-up, confirm, forgotten password, reset password) SHALL be decorated with `[AllowAnonymous]` so unauthenticated users can access them.

#### Scenario: Anonymous access to auth pages
- **GIVEN** an unauthenticated user
- **WHEN** the user navigates to any auth page
- **THEN** the page renders without requiring authentication
