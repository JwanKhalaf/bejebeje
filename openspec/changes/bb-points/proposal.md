## Summary

BB Points is a gamification system that awards logged-in Bejebeje users points for contributing artists, lyrics, and reports. It introduces a local users table with materialized point totals, a points engine with daily submission limits, contributor labels, points display across the nav bar, profile pages, and lyric detail pages, a retroactive calculation for existing users, and a data contract for the admin project to award approval points.

## Why

Bejebeje relies on community contributions (artists, lyrics, reports) but offers no recognition or incentive for participation. There is no visible reward, which limits engagement and motivation. BB Points introduces a gamification system that rewards logged-in users for contributing and surfaces their impact across the site.

## What Changes

- Add a points engine that awards BB Points for defined actions (submissions, approvals, likes) with fixed point values
- Add a local `users` table that caches Cognito usernames and stores materialized per-category point totals, eliminating per-page-load Cognito API calls
- Add a `point_events` table for audit trail and activity feed
- Add configurable daily submission limits that throttle point earning (not submissions themselves)
- Add a nav bar View Component showing total points, contributor label, and a points-changed indicator for authenticated users
- Modify the own profile page (`/profile`) to show per-category breakdown and recent activity feed
- Add a new public profile page (`/profile/{username}`) showing total points, contributor label, and contribution counts
- Modify the lyric detail page to show submitter's points, contributor label, and link to their public profile
- Add TempData-driven notification banners for daily limit and points-earned messages
- Modify the report submission flow to remove the blocking rate limit (always allow submission, skip points when over limit)
- Add a standalone console application (`Bejebeje.Retroactive`) for one-time retroactive point calculation for existing users
- Define a data contract for the `bejebeje.admin` project to award approval points and send notification emails
- Add contributor labels (New Contributor, Contributor, Regular Contributor, Top Contributor) derived from point thresholds
- Sync user records on login via the OIDC `OnTokenValidated` event

## Capabilities

### New Capabilities
- `points-data-model`: Domain entities (User, PointEvent), PointActionType enum, point value constants, contributor label thresholds, BbPointsOptions configuration
- `user-record-sync`: Authentication event handler that ensures a local user record exists and username is current on every login
- `points-engine`: Core IBbPointsService — award submission points with daily limit checks, award approval points idempotently, query points data for display
- `nav-bar-points`: BbPointsNavViewComponent rendering points total, contributor label, and points-changed indicator in the navigation bar
- `submission-points-flow`: Controller-level integration for awarding points after artist, lyric, and report submissions, including daily limit messaging via TempData
- `own-profile`: Own profile page showing per-category breakdown, like-based points, total, contributor label, recent activity feed, and indicator dismissal
- `public-profile`: Public profile page at `/profile/{username}` showing total points, contributor label, and contribution counts
- `lyric-detail-points`: Submitter's points total, contributor label, and clickable profile link on lyric detail pages
- `retroactive-calculation`: Standalone console application for one-time retroactive point calculation for all existing users
- `admin-approval-contract`: Data contract defining how the bejebeje.admin project awards approval/acknowledgement points and sends notification emails

### Modified Capabilities
(No existing OpenSpec capabilities to modify)

## Impact

- **Bejebeje.Domain**: New `User` entity, `PointEvent` entity, `PointActionType` enum
- **Bejebeje.DataAccess**: New DbSet entries, EF Core configuration for new entities, two new database migrations
- **Bejebeje.Services**: New `IBbPointsService`/`BbPointsService`, new `BbPointsOptions`/`DailyLimitsOptions` configuration classes
- **Bejebeje.Models**: New ViewModels (NavBarPointsViewModel, OwnProfileViewModel, PublicProfileViewModel, PointActivityViewModel, SubmitterPointsViewModel)
- **Bejebeje.Mvc**: Modified controllers (Profile, Artist, Lyric, Report), new View Component, new shared partial, modified layout and views, modified Program.cs for DI and auth event
- **New project**: `Bejebeje.Retroactive` console application
- **Cross-project**: Data contract for `bejebeje.admin` (shared database, new tables)
- **Database**: Two new tables (`users`, `point_events`), no changes to existing tables
- **Configuration**: New `BbPoints` section in `appsettings.json`

## References

- [REQUIREMENTS.md](../../REQUIREMENTS.md) — Intent, scope, constraints, success criteria, and risk analysis
- [SPEC.md](../../SPEC.md) — Authoritative implementation-ready technical specification (source of truth)
