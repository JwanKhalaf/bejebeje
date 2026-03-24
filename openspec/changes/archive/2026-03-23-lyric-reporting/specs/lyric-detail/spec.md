## MODIFIED Requirements

### Requirement: Flag icon added to lyric detail page for approved lyrics
The lyric detail page (`Views/Lyric/Lyric.cshtml`) SHALL display a flag SVG icon in the right-side column (in the `<div class="flex flex-col gap-4">` container alongside the verified badge and like button). The icon SHALL be positioned below the like button, wrapped in an `<a>` tag linking to `/artists/{artistSlug}/lyrics/{lyricSlug}/report`. The icon SHALL only be rendered for approved lyrics (check `Model.IsApproved`). The icon SHALL be visible to all users (authenticated and anonymous). The icon SHALL be styled consistently with existing icons (`size-10 lg:size-8`, `text-neutral-500`).

#### Scenario: Flag icon is visible on an approved lyric page
- **GIVEN** a user (authenticated or anonymous) navigates to an approved lyric's detail page
- **WHEN** the page renders
- **THEN** a flag SVG icon is displayed in the right-side column below the like button, linking to the report page

#### Scenario: Flag icon is not rendered for unapproved lyrics
- **GIVEN** a lyric exists but is not approved
- **WHEN** the lyric detail page renders
- **THEN** no flag icon is displayed

#### Scenario: Flag icon links to the correct report URL
- **GIVEN** a lyric by artist "adnan-karim" with slug "bo-min"
- **WHEN** the flag icon is rendered
- **THEN** the icon's href is `/artists/adnan-karim/lyrics/bo-min/report`

#### Scenario: Flag icon styling is consistent with other icons
- **GIVEN** the flag icon is rendered on the lyric detail page
- **WHEN** inspecting the icon's classes
- **THEN** the icon uses `size-10 lg:size-8` and `text-neutral-500` classes matching the verified and like icons

#### Scenario: No controller or service changes are needed for the flag icon
- **GIVEN** the existing lyric detail view already has `Model.Artist.PrimarySlug`, `Model.PrimarySlug`, and `Model.IsApproved`
- **WHEN** the flag icon is added to the view
- **THEN** no changes to LyricController or any service are required
