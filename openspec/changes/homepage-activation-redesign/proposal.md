## Summary

Redesign the Bejebeje homepage from a passive browse surface into a contribution activation surface that converts cultural interest into first contributions, replacing stale activity-led modules with contribution-led modules that work regardless of platform velocity.

## Why

The current homepage leads with activity-based modules (recent submissions, recently verified, popular female artists) that signal inactivity on a quiet platform. It has no contribution prompts, no participation thesis, and no bridge from reading to acting. This weakens social proof and discourages participation.

## What Changes

- **New homepage layout** with 6 sections: hero area with contribution CTAs, "help grow the archive" task cards, inline contribution explainer, "where help is needed" opportunity cards, community impact stats block, and curated discovery section.
- **New `IHomepageService`** that orchestrates homepage-specific queries and caching, including opportunity card queries, cached community impact stats, and corrected female artists query.
- **New view models** (`HomepageViewModel`, `OpportunityCardViewModel`, `CommunityImpactStatsViewModel`) replacing the existing `IndexViewModel`.
- **Artist detail page CTA** — lightweight "Know another lyric by this artist? Add one" link.
- **Lyric detail page copy strengthening** — "Spot an error? Report it" text alongside existing report icon.
- **Auth redirect flow** — anonymous CTA clicks redirect to login with `returnUrl` preserving source context for GA4 attribution.
- **Sidebar label change** — "Sign up" becomes "Join as a contributor" for unauthenticated users.
- **Login/signup page copy** — contribution-framing message added to both pages.
- **GA4 event instrumentation** — 9 custom events with stable constants module covering the full activation funnel.
- **Removed modules** — "Recent submissions" and "Recently verified" modules removed from homepage.

## Capabilities

### New Capabilities
- `homepage-service`: IHomepageService with opportunity card queries (Q1/Q2), community impact stats (Q3, cached), corrected female artists query (Q4), and HomepageViewModel composition.
- `homepage-view`: Redesigned homepage view with hero, help grow archive, inline explainer, where help is needed, community impact stats, and curated discovery sections.
- `artist-page-cta`: Lightweight contribution CTA on artist detail page with auth-variant behavior.
- `lyric-page-copy`: Copy strengthening on lyric detail page for report affordance.
- `auth-redirect-flow`: Authentication redirect preserving source context through login/signup round-trip for GA4 attribution.
- `navigation-and-copy`: Sidebar navigation label change and login/signup page contribution messaging.
- `ga4-analytics`: Client-side GA4 event instrumentation with analytics constants module covering 9 v1 events.

### Modified Capabilities

## Scope

- **Presentation layer:** HomeController, homepage view, artist detail view, lyric detail view, login view, signup view, sidebar layout.
- **Service layer:** New IHomepageService, minor correction to IArtistsService.GetTopTenFemaleArtistsByLyricsCountAsync().
- **Client-side:** New analytics constants JS module, event firing via existing gtag.js.
- **Unchanged:** All submission forms, approval workflows, BB Points logic, Cognito integration, API endpoints, email service, image service, sitemap service, database schema.

## References

- [REQUIREMENTS.md](../../REQUIREMENTS.md)
- [SPEC.md](../../SPEC.md)
- [analytics-and-event-tracking.md](../../analytics-and-event-tracking.md)
