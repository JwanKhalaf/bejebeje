## ADDED Requirements

### Requirement: Lyric detail page strengthens report affordance with visible text
The lyric detail page (`/artists/{artistSlug}/lyrics/{lyricSlug}`) SHALL strengthen the existing report link with visible text copy: "Spot an error? Report it." The existing flag icon SHALL be retained; the text label SHALL be added alongside it. The link target SHALL remain the existing report route. This SHALL be a copy change only — no major layout changes.

#### Scenario: Report text label appears alongside existing icon
- **GIVEN** a user views a lyric detail page
- **WHEN** the report affordance renders
- **THEN** the text "Spot an error? Report it" SHALL appear alongside the existing flag icon, linking to the existing report route

#### Scenario: Auth redirect behaviour unchanged for reports
- **GIVEN** an anonymous user clicks the report link on the lyric detail page
- **WHEN** the navigation occurs
- **THEN** the existing `[Authorize]` attribute SHALL redirect to `/login` automatically (no new redirect logic needed)
