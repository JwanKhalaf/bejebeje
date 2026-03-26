## ADDED Requirements

### Requirement: Artist detail page displays contribution CTA
The artist detail page (`/artists/{artistSlug}/lyrics`) SHALL display a lightweight contribution CTA: "Know another lyric by this artist? Add one." The CTA SHALL be placed below the artist info / above the lyrics list. This SHALL be a copy and link addition only — no major layout changes.

#### Scenario: Authenticated user sees direct add-lyric link
- **GIVEN** an authenticated user views an artist detail page for artist with slug "sivan-perwer"
- **WHEN** the CTA renders
- **THEN** the CTA text SHALL be "Know another lyric by this artist? Add one" and the link SHALL point to `/artists/sivan-perwer/lyrics/new`

#### Scenario: Anonymous user sees CTA linking through login redirect
- **GIVEN** an anonymous user views an artist detail page for artist with slug "sivan-perwer"
- **WHEN** the CTA renders
- **THEN** the link SHALL point to `/login?returnUrl=%2Fartists%2Fsivan-perwer%2Flyrics%2Fnew%3Fentry_point%3Dartist_header_add_lyric%26contribution_type%3Dlyric`

#### Scenario: CTA placement does not restructure layout
- **GIVEN** the artist detail page renders
- **WHEN** the CTA is present
- **THEN** it SHALL appear below the artist info and above the lyrics list without altering the existing layout structure
