## MODIFIED Requirements

### Requirement: The lyric detail page SHALL display the submitter's BB points and contributor label
On the lyric detail page, the submitter's total BB points and contributor label SHALL be displayed next to their username. The controller SHALL call `IBbPointsService.GetSubmitterPointsAsync(submitterUserId)` and add the result to the view model.

#### Scenario: Submitter points displayed on lyric detail page
- **GIVEN** a lyric submitted by user "songwriter" with TotalPoints=210
- **WHEN** the lyric detail page renders
- **THEN** the submitter's username "songwriter" is shown with "210" points and "Regular Contributor" label

#### Scenario: Submitter with no user record shows fallback values
- **GIVEN** a lyric submitted by a user who has no local user record (pre-launch, missed by retroactive script)
- **WHEN** the lyric detail page renders
- **THEN** the submitter's username is resolved from Cognito, displayed with 0 points and "New Contributor" label

### Requirement: The submitter's username SHALL link to their public profile
The submitter's username on the lyric detail page SHALL be a clickable link to `/profile/{username}`. New properties SHALL be added to the lyric details view model: SubmitterPoints (int), SubmitterLabel (string), SubmitterProfileUrl (string).

#### Scenario: Username links to public profile
- **GIVEN** a lyric submitted by user "songwriter"
- **WHEN** the lyric detail page renders
- **THEN** the username "songwriter" is a clickable link to `/profile/songwriter`

#### Scenario: Profile link for user without record leads to 404 until they log in
- **GIVEN** a lyric submitted by a user who has no local user record
- **WHEN** the username link is clicked
- **THEN** the link navigates to `/profile/{username}` which returns 404 (self-corrects when the user logs in)
