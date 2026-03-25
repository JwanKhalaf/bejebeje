## ADDED Requirements

### Requirement: Rate limiting on login and sign-up POST endpoints
The system SHALL rate-limit `POST /login` and `POST /signup` to 2 requests per minute per client IP address using ASP.NET Core's built-in `AddRateLimiter` with fixed-window policies. When exceeded, the system SHALL return HTTP 429 (Too Many Requests). The `UseRateLimiter()` middleware SHALL be added to the pipeline after `UseRouting()` and before `UseAuthentication()`.

#### Scenario: Login rate limit not exceeded
- **GIVEN** a client IP has made fewer than 2 login POST requests in the current minute
- **WHEN** the client submits a login POST
- **THEN** the request proceeds normally

#### Scenario: Login rate limit exceeded
- **GIVEN** a client IP has made 2 login POST requests in the current minute
- **WHEN** the client submits a third login POST
- **THEN** the system returns HTTP 429

#### Scenario: Sign-up rate limit exceeded
- **GIVEN** a client IP has made 2 sign-up POST requests in the current minute
- **WHEN** the client submits a third sign-up POST
- **THEN** the system returns HTTP 429

#### Scenario: Rate limits are per-IP
- **GIVEN** two different client IPs
- **WHEN** each makes 2 login POST requests in the same minute
- **THEN** both are within their limits (no 429)

#### Scenario: Rate limit window resets
- **GIVEN** a client IP exceeded the rate limit
- **WHEN** the fixed window (1 minute) elapses
- **THEN** the client can make requests again
