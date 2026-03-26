## ADDED Requirements

### Requirement: Homepage hero area with search and contribution CTAs
The homepage SHALL display a hero area containing: a headline framing the platform's purpose around preserving Kurdish lyrics collaboratively, supporting text explaining what users can do (search, discover, contribute), a search input retaining current search functionality, a primary CTA to add an artist, and a secondary CTA to contribute a lyric. The hero headline SHALL use an `<h1>` tag.

#### Scenario: Anonymous user sees hero with join-phrased CTAs
- **GIVEN** an anonymous user visits the homepage
- **WHEN** the hero area renders
- **THEN** the primary CTA SHALL read "Join to add an artist" linking to `/login?returnUrl=...` and the secondary CTA SHALL read "Join to contribute a lyric" linking to `/login?returnUrl=...`

#### Scenario: Authenticated user sees hero with direct action CTAs
- **GIVEN** an authenticated user visits the homepage
- **WHEN** the hero area renders
- **THEN** the primary CTA SHALL read "Add an artist" linking to `/artists/new/selector` and the secondary CTA SHALL read "Contribute a lyric" linking to `/artists`

#### Scenario: Search input retains current functionality
- **GIVEN** a user enters a search term in the hero search input
- **WHEN** the user submits the search
- **THEN** the existing search behaviour SHALL execute unchanged

### Requirement: Help grow the archive section with task cards
The homepage SHALL display a "Help grow the archive" section with task cards: add a missing artist, add lyrics for an existing artist, report an issue with a lyric, and create an account (anonymous only). Each card SHALL include a one-line description of the action. Section heading SHALL use `<h2>`.

#### Scenario: Anonymous user sees four task cards including create account
- **GIVEN** an anonymous user visits the homepage
- **WHEN** the "Help grow the archive" section renders
- **THEN** 4 task cards SHALL be displayed: add artist, add lyric, report issue, and create account

#### Scenario: Authenticated user sees three task cards without create account
- **GIVEN** an authenticated user visits the homepage
- **WHEN** the "Help grow the archive" section renders
- **THEN** the "Create an account" card SHALL be hidden, showing only 3 actionable contribution options

#### Scenario: Report issue card is instructional
- **GIVEN** any user visits the homepage
- **WHEN** the report issue task card renders
- **THEN** the card SHALL explain that reporting is available on any lyric page and link to `/artists` as a discovery entry point (not a direct report link since no lyric context exists on homepage)

### Requirement: Inline contribution explainer block
The homepage SHALL include a compact inline explainer block communicating the 4-step contribution workflow: (1) submit content, (2) admins review, (3) approved contributions appear publicly and are credited, (4) contributors earn BB Points. This SHALL be rendered inline, not as a separate page. BB Points SHALL appear as a supporting benefit in the final step, not as the primary motivation. The tone SHALL feel like "here's how you help preserve Kurdish music."

#### Scenario: Explainer renders inline on homepage
- **GIVEN** a user visits the homepage
- **WHEN** the explainer section renders
- **THEN** it SHALL display 4 steps inline on the page with BB Points appearing only in the final step as supporting motivation

### Requirement: Where help is needed section with opportunity cards
The homepage SHALL display a "Where help is needed" section showing up to 8 opportunity cards. Each card SHALL display the artist name and link to the artist detail page. The section SHALL use invitation framing (e.g. "Help complete these artist pages"). Section heading SHALL use `<h2>`.

#### Scenario: Opportunity cards render with artist name and link
- **GIVEN** IHomepageService returns 5 opportunity cards
- **WHEN** the "Where help is needed" section renders
- **THEN** 5 cards SHALL be displayed, each showing artist name and linking to `/artists/{artistSlug}/lyrics`

#### Scenario: Zero opportunities shows positive fallback message
- **GIVEN** IHomepageService returns 0 opportunity cards
- **WHEN** the "Where help is needed" section renders
- **THEN** the section heading SHALL still render and a positive message SHALL display: "The archive is in great shape — but there are always more artists to add!" with a link to the add-artist flow

#### Scenario: Opportunity cards show artist image placeholder when no image
- **GIVEN** an opportunity card for an artist with has_image=false
- **WHEN** the card renders
- **THEN** a placeholder/default artist avatar SHALL be displayed using the existing image helper fallback

### Requirement: Community impact stats block always displayed
The homepage SHALL display a community impact block with three stats: total approved lyrics, total approved artists, and total contributors. The block SHALL always be displayed regardless of how small the counts are. Framing copy SHALL make small numbers purposeful (e.g. "X lyrics preserved and growing", "X artists documented so far", "X contributors and counting").

#### Scenario: Stats render with positive framing copy
- **GIVEN** community impact stats show 12 lyrics, 5 artists, 3 contributors
- **WHEN** the stats block renders
- **THEN** all three stats SHALL be displayed with contextualising framing copy that makes small numbers feel purposeful

#### Scenario: Stats section hidden when CommunityImpact is null
- **GIVEN** CommunityImpact on the view model is null (database failure)
- **WHEN** the homepage renders
- **THEN** the community impact section SHALL be hidden entirely

### Requirement: Curated discovery section with editorial framing
The homepage SHALL include a curated discovery section showing the top 10 female artists by lyric count with editorial framing (e.g. "Women of Kurdish music"), not a raw data listing. Section heading SHALL use `<h2>`.

#### Scenario: Female artists render with editorial framing
- **GIVEN** Q4 returns 8 female artists
- **WHEN** the curated discovery section renders
- **THEN** 8 artist entries SHALL be displayed under editorial heading (e.g. "Women of Kurdish music")

#### Scenario: Zero female artists hides curated section
- **GIVEN** Q4 returns 0 female artists
- **WHEN** the homepage renders
- **THEN** the curated discovery section SHALL be hidden entirely

### Requirement: Recent submissions and recently verified modules removed
The "Recent submissions" module and the "Recently verified" module SHALL be removed from the homepage.

#### Scenario: Old modules no longer render
- **GIVEN** a user visits the homepage
- **WHEN** the page renders
- **THEN** no "Recent submissions" module and no "Recently verified" module SHALL be present

### Requirement: Responsive layout within existing structure
All new homepage sections SHALL work on both desktop and mobile layouts, respecting the existing sidebar (desktop) / bottom-nav (mobile) responsive pattern. Tailwind responsive breakpoints (`lg:` prefix) SHALL be used consistent with the existing layout.

#### Scenario: Homepage sections render correctly on mobile
- **GIVEN** a user views the homepage on a mobile device
- **WHEN** the page renders
- **THEN** all sections SHALL be responsive and the existing bottom navigation SHALL remain intact

### Requirement: Semantic HTML and accessibility
All new homepage elements SHALL meet WCAG AA standards. All CTAs SHALL be keyboard-navigable and have accessible names. Artist images SHALL have alt text (artist name). Colour contrast SHALL meet AA ratios. Semantic HTML SHALL be used: `<section>` for each section, `<h1>` for hero, `<h2>` for section headings, `<ul>`/`<li>` for lists, `<a>` for links.

#### Scenario: Opportunity cards use semantic list markup
- **GIVEN** opportunity cards are rendered
- **WHEN** the HTML is inspected
- **THEN** cards SHALL be wrapped in `<ul>`/`<li>` elements within a `<section>` and artist images SHALL have alt text set to the artist name

#### Scenario: All CTAs are keyboard navigable
- **GIVEN** a keyboard user navigates the homepage
- **WHEN** they tab through interactive elements
- **THEN** all CTAs SHALL be focusable and activatable via keyboard

### Requirement: SEO heading hierarchy
The homepage SHALL use `<h1>` for the hero headline and `<h2>` for each section heading, maintaining a valid heading hierarchy for SEO.

#### Scenario: Heading hierarchy is valid
- **GIVEN** the homepage renders
- **WHEN** the HTML heading structure is inspected
- **THEN** there SHALL be exactly one `<h1>` (hero) followed by `<h2>` tags for each section
