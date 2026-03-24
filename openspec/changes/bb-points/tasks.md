## 1. Database Migrations

- [ ] 1.1 **[BLOCKED — user must create migration]** A database migration is required. Create `users` table with columns: id (int, PK, auto-increment), cognito_user_id (text, UNIQUE, NOT NULL), username (text, UNIQUE, NOT NULL), artist_submission_points (int, NOT NULL, DEFAULT 0), artist_approval_points (int, NOT NULL, DEFAULT 0), lyric_submission_points (int, NOT NULL, DEFAULT 0), lyric_approval_points (int, NOT NULL, DEFAULT 0), report_submission_points (int, NOT NULL, DEFAULT 0), report_acknowledgement_points (int, NOT NULL, DEFAULT 0), last_seen_points (int, NOT NULL, DEFAULT 0), created_at (timestamp, NOT NULL), modified_at (timestamp, NULL). Add UNIQUE indexes on cognito_user_id and username. Ref: SPEC §5 New Entity: User.
- [ ] 1.2 **[BLOCKED — user must create migration]** A database migration is required. Create `point_events` table with columns: id (int, PK, auto-increment), user_id (int, FK to users.id, NOT NULL), action_type (int, NOT NULL), points (int, NOT NULL), entity_id (int, NOT NULL), entity_name (text, NOT NULL), created_at (timestamp, NOT NULL). Add index ix_point_events_user_id_created_at on (user_id, created_at DESC). Add UNIQUE index ix_point_events_user_id_action_type_entity_id on (user_id, action_type, entity_id). Ref: SPEC §5 New Entity: PointEvent.

## 2. Domain Entities & Enums

- [x] 2.1 Create `PointActionType` enum in Bejebeje.Domain with values: ArtistSubmitted=1, ArtistApproved=2, LyricSubmitted=3, LyricApproved=4, ReportSubmitted=5, ReportAcknowledged=6. Ref: SPEC §5 New Enum: PointActionType.
- [x] 2.2 Create `User` entity in Bejebeje.Domain with all fields matching the data model: Id, CognitoUserId, Username, six category point columns, LastSeenPoints, CreatedAt, ModifiedAt. Ref: SPEC §5 New Entity: User.
- [x] 2.3 Create `PointEvent` entity in Bejebeje.Domain with all fields: Id, UserId, ActionType (PointActionType), Points, EntityId, EntityName, CreatedAt. Include navigation property to User. Ref: SPEC §5 New Entity: PointEvent.
- [x] 2.4 Define point value constants in a static class (e.g., `BbPointsConstants`) in Bejebeje.Domain: ArtistSubmittedNoPhoto=1, ArtistSubmittedWithPhoto=5, ArtistApprovedNoPhoto=9, ArtistApprovedWithPhoto=10, LyricSubmitted=5, LyricApproved=15, ReportSubmitted=1, ReportAcknowledged=4. Ref: SPEC §5 Point Values.
- [x] 2.5 Define contributor label thresholds and a static method to derive a label string from a total points integer: 0-49="New Contributor", 50-199="Contributor", 200-499="Regular Contributor", 500+="Top Contributor". Ref: SPEC §5 Contributor Labels.

## 3. Data Access Configuration

- [x] 3.1 Add EF Core entity configuration for `User` entity: table name "users", snake_case column naming, UNIQUE constraints on CognitoUserId and Username, appropriate column types. Register as DbSet on the DbContext. Ref: SPEC §5 New Entity: User.
- [x] 3.2 Add EF Core entity configuration for `PointEvent` entity: table name "point_events", FK relationship to User, index on (UserId, CreatedAt DESC), UNIQUE index on (UserId, ActionType, EntityId), snake_case column naming. Register as DbSet on the DbContext. Ref: SPEC §5 New Entity: PointEvent.

## 4. Configuration Classes

- [x] 4.1 Create `DailyLimitsOptions` class in Bejebeje.Services with int properties: ArtistSubmissions (default 5), LyricSubmissions (default 10), ReportSubmissions (default 5). Ref: SPEC §6 Configuration: BbPointsOptions.
- [x] 4.2 Create `BbPointsOptions` class in Bejebeje.Services with a `DailyLimits` property of type `DailyLimitsOptions`. Ref: SPEC §6 Configuration: BbPointsOptions.
- [x] 4.3 Add the `BbPoints` configuration section to `appsettings.json` with default daily limit values: ArtistSubmissions=5, LyricSubmissions=10, ReportSubmissions=5. Ref: SPEC §10 Configuration.
- [x] 4.4 Register `BbPointsOptions` in `Program.cs` via `builder.Services.Configure<BbPointsOptions>(builder.Configuration.GetSection("BbPoints"))`. Ref: SPEC §6 Configuration: BbPointsOptions.

## 5. View Models

- [x] 5.1 Create `NavBarPointsViewModel` in Bejebeje.Models with properties: TotalPoints (int), ContributorLabel (string), HasPointsChanged (bool). Ref: SPEC §6 View Component: BbPointsNavViewComponent.
- [x] 5.2 Create `OwnProfileViewModel` in Bejebeje.Models with properties: per-category point totals (6 ints), LikePoints (int), TotalPoints (int), ContributorLabel (string), Username (string), RecentActivity (list of PointActivityViewModel). Ref: SPEC §6 IBbPointsService.GetOwnProfileDataAsync.
- [x] 5.3 Create `PublicProfileViewModel` in Bejebeje.Models with properties: Username (string), TotalPoints (int), ContributorLabel (string), ArtistsSubmittedCount (int), LyricsSubmittedCount (int). Ref: SPEC §6 IBbPointsService.GetPublicProfileDataAsync.
- [x] 5.4 Create `PointActivityViewModel` in Bejebeje.Models with properties: ActionDescription (string), EntityName (string), Points (int), Date (DateTime). Ref: SPEC §7 Flow 8.
- [x] 5.5 Create `SubmitterPointsViewModel` in Bejebeje.Models with properties: TotalPoints (int), ContributorLabel (string), Username (string). Ref: SPEC §6 IBbPointsService.GetSubmitterPointsAsync.

## 6. Points Engine Service

- [x] 6.1 Create `IBbPointsService` interface in Bejebeje.Services with all method signatures: EnsureUserExistsAsync, GetNavBarDataAsync, GetOwnProfileDataAsync, GetPublicProfileDataAsync, GetSubmitterPointsAsync, AwardSubmissionPointsAsync, AwardApprovalPointsAsync, GetDailySubmissionCountAsync. Ref: SPEC §6 Service Interface: IBbPointsService.
- [x] 6.2 Implement `BbPointsService.EnsureUserExistsAsync`: query users by cognito_user_id. If not found, insert new record with all zeros. If found and username differs, update username and ModifiedAt. Return the User. Ref: SPEC §7 Flow 1.
- [x] 6.3 Implement `BbPointsService.GetDailySubmissionCountAsync`: count submissions from the specified source table (artists, lyrics, or lyric_reports) for the given user_id on the given UTC date. Use raw SQL or EF Core to query the source table where user_id matches and created_at falls within the UTC day. Ref: SPEC §6 IBbPointsService.GetDailySubmissionCountAsync.
- [x] 6.4 Implement `BbPointsService.AwardSubmissionPointsAsync`: call GetDailySubmissionCountAsync to check the limit, determine which daily limit to use based on action type, if within limit insert PointEvent and increment the relevant category column on the User record in a single transaction, return true. If over limit, return false. Ref: SPEC §7 Flow 2 steps 6a-6d.
- [x] 6.5 Implement `BbPointsService.AwardApprovalPointsAsync`: call EnsureUserExistsAsync, insert PointEvent using INSERT ON CONFLICT DO NOTHING (or catch unique violation), if insert succeeded increment the relevant category column on the User record. No daily limit check. Ref: SPEC §7 Flow 5.
- [x] 6.6 Implement `BbPointsService.GetNavBarDataAsync`: query user record by cognito_user_id, sum 6 category columns, compute like_points via COUNT from likes table, compute total, derive contributor label, compare total to LastSeenPoints for HasPointsChanged. Return NavBarPointsViewModel. Ref: SPEC §7 Flow 10.
- [x] 6.7 Implement `BbPointsService.GetOwnProfileDataAsync`: query user record, read per-category totals, compute like_points, compute total, derive label, query 20 most recent PointEvents ordered by created_at DESC, update LastSeenPoints = total, return OwnProfileViewModel. Ref: SPEC §7 Flow 8.
- [x] 6.8 Implement `BbPointsService.GetPublicProfileDataAsync`: look up user by username. If not found, return null. Compute total and label. Count artists submitted (SELECT COUNT from artists WHERE user_id=cognitoUserId AND is_deleted=false). Count lyrics submitted (SELECT COUNT from lyrics WHERE user_id=cognitoUserId AND is_deleted=false). Return PublicProfileViewModel. Ref: SPEC §7 Flow 9.
- [x] 6.9 Implement `BbPointsService.GetSubmitterPointsAsync`: look up user by cognito_user_id. If found, compute total, derive label, return SubmitterPointsViewModel. If not found, return fallback (0 points, "New Contributor", resolve username via ICognitoService.GetPreferredUsernameAsync). Ref: SPEC §7 Flow 11.
- [x] 6.10 Register `IBbPointsService` / `BbPointsService` as scoped in `Program.cs` DI container. Ref: SPEC §6 Service Interface.

## 7. User Record Sync on Login

- [x] 7.1 Add the `profile` scope to the OIDC configuration in `Program.cs` alongside existing `openid` and `email` scopes. Ref: SPEC §6 Authentication Event, §11 Deferred Decision 2.
- [x] 7.2 Add an `OnTokenValidated` event handler in the OIDC configuration in `Program.cs`: extract cognitoUserId from the `sub` claim, obtain preferredUsername from the `preferred_username` claim (or fallback to ICognitoService.GetPreferredUsernameAsync), call IBbPointsService.EnsureUserExistsAsync(cognitoUserId, preferredUsername). Ref: SPEC §7 Flow 1.

## 8. Nav Bar View Component

- [x] 8.1 Create `BbPointsNavViewComponent` in Bejebeje.Mvc/ViewComponents: check IsAuthenticated, if false return Content.Empty. If true, extract cognitoUserId from sub claim, call GetNavBarDataAsync, return view with NavBarPointsViewModel. Ref: SPEC §6 View Component.
- [x] 8.2 Create the View Component Razor view: render a `<li>` element matching existing nav item pattern with the diamond SVG icon (using size-5 lg:size-8 text-neutral-800 classes), total points number, linked to /profile. When HasPointsChanged=true, apply visual indicator (glow/dot) to the diamond icon. Ref: SPEC §6 View Component rendered output.
- [x] 8.3 Add error handling to the View Component: wrap the service call in a try/catch, log exceptions at ERROR level, and return Content.Empty on failure so the page renders without points. Ref: SPEC §9 Nav Bar View Component Failure.
- [x] 8.4 Invoke the BbPointsNavViewComponent from `_Layout.cshtml` using `@await Component.InvokeAsync("BbPointsNav")` within the existing nav `<ul>`. Ref: SPEC §7 Flow 10.

## 9. Daily Limit Messaging

- [x] 9.1 Create `_PointsNotification.cshtml` shared partial view that reads TempData keys: `BbPoints:Earned` (bool), `BbPoints:Amount` (int), `BbPoints:EntityType` (string). If Earned=true, render success banner ("You earned [Amount] BB Points for your [EntityType] submission!"). If Earned=false, render limit banner ("Your submission has been saved! You've already earned your maximum BB points for [EntityType] submissions today. Come back tomorrow to earn more!"). The banner should be a dismissible full-width element positioned at the top of the main content area (below the nav bar, above page content), consistent with standard notification banner patterns. Ref: SPEC §6 TempData Contract.
- [x] 9.2 Include `_PointsNotification.cshtml` in `_Layout.cshtml` immediately after the nav bar and before the main content container, so it renders on all pages at the top of the content area. TempData auto-expires after being read so the banner displays once. Ref: SPEC §6 TempData Contract.

## 10. Artist Submission Points Integration

- [x] 10.1 Modify `ArtistController.Create` (individual artist POST action): after the image upload step completes (or is skipped), determine hasImage from whether the photo upload succeeded, call AwardSubmissionPointsAsync with ActionType=ArtistSubmitted and points = hasImage ? 5 : 1, set TempData keys (BbPoints:Earned, BbPoints:Amount, BbPoints:EntityType="artist"). Ref: SPEC §7 Flow 2 steps 3-7.
- [x] 10.2 Modify `ArtistController.CreateBand` (band artist POST action): apply the same points-awarding logic as individual artist creation. Ref: SPEC §7 Flow 2, FR-2.

## 11. Lyric Submission Points Integration

- [x] 11.1 Modify `LyricController.Create` POST action: after lyric and slug creation, call AwardSubmissionPointsAsync with ActionType=LyricSubmitted, Points=5, set TempData keys with EntityType="lyric". Ensure TempData is not consumed by the Like action redirect. Ref: SPEC §7 Flow 3.
- [x] 11.2 Verify that the `LyricController.Like` action does not read or enumerate BB Points TempData keys (BbPoints:*), so TempData survives through the double redirect (Create -> Like -> Lyric detail). Ref: SPEC §7 Flow 3 Implementation Constraint.

## 12. Report Submission Points Integration

- [x] 12.1 Remove the existing daily limit check from `ReportController.Report` GET action that redirects to the LimitReached page. The report form SHALL always be shown (duplicate pending report check remains). Ref: SPEC §7 Flow 4 step 2.
- [x] 12.2 Remove the existing `reportCount >= 3` check from `ReportController.SubmitReport` POST action that redirects to LimitReached. Ref: SPEC §7 Flow 4 step 4c.
- [x] 12.3 Add point awarding to `ReportController.SubmitReport` POST action: after saving the report via CreateReportAsync, call AwardSubmissionPointsAsync with ActionType=ReportSubmitted, Points=1, set TempData keys with EntityType="report". Ref: SPEC §7 Flow 4 steps 4e-4g.
- [x] 12.4 Remove the LimitReached route and its associated Razor view from the ReportController. Requests to the old route should return 404. Ref: SPEC §7 Flow 4 Day 1 Verification.

## 13. Own Profile Page

- [x] 13.1 Modify `ProfileController.Index` action: call IBbPointsService.GetOwnProfileDataAsync(cognitoUserId), pass the OwnProfileViewModel to the view. Ref: SPEC §7 Flow 8.
- [x] 13.2 Update `Profile/Index.cshtml` to display the per-category points breakdown table showing all 6 event-based categories, like-based points, and total. Display the contributor label. Retain the existing "Want to help?" content. Ref: SPEC §7 Flow 8 step 5.
- [x] 13.3 Add the "Recent Activity" section to `Profile/Index.cshtml`: render the 20 most recent PointEvents showing action description, entity name, points earned, and date in reverse chronological order. Ref: SPEC §7 Flow 8.
- [x] 13.4 Implement the activity feed empty state: when there are 0 PointEvents and category totals are all 0, show "You haven't earned any BB Points yet. Submit an artist or lyric to get started!" When there are 0 PointEvents but category totals > 0 (retroactive only), show "Your historical contributions have been counted! New activity will appear here as you earn more points." Ref: SPEC §7 Flow 8 Empty State.

## 14. Public Profile Page

- [x] 14.1 Add a `Public` action to `ProfileController` at route `/profile/{username}`: if the authenticated user's username matches the route parameter, redirect to `/profile`. Otherwise, call GetPublicProfileDataAsync(username). If null, return 404. Otherwise, render the public profile view. This action SHALL NOT have an [Authorize] attribute (anonymous users can view). Ref: SPEC §7 Flow 9.
- [x] 14.2 Create the public profile Razor view: display the username, total BB points, contributor label, and contribution counts labelled "artists submitted" and "lyrics submitted". No category breakdown or activity feed. Ref: SPEC §7 Flow 9 step 5.
- [x] 14.3 Ensure routing for `/profile/{username}` and `/profile` does not conflict. `/profile` (no parameter) renders own profile, `/profile/{username}` renders public profile. Ref: SPEC §7 Flow 9 Day 1 Verification.

## 15. Lyric Detail Page Integration

- [x] 15.1 Modify the `LyricController.Lyric` action: after fetching lyric details, call IBbPointsService.GetSubmitterPointsAsync(submitterUserId). Add new properties to the lyric details view model: SubmitterPoints (int), SubmitterLabel (string), SubmitterProfileUrl (string, formatted as /profile/{username}). Ref: SPEC §7 Flow 11.
- [x] 15.2 Add SubmitterPoints, SubmitterLabel, and SubmitterProfileUrl properties to the existing lyric details view model. Ref: SPEC §7 Flow 11 step 4.
- [x] 15.3 Update `Lyric/Lyric.cshtml`: render the submitter's username as a clickable link to their public profile URL, with BB points total and contributor label displayed next to the username. Ref: SPEC §7 Flow 11 step 5.

## 16. Retroactive Calculation Console App

- [ ] 16.1 Create a new `Bejebeje.Retroactive` console application project in the solution. It SHALL accept a connection string via command-line argument or environment variable and connect to the PostgreSQL database. Ref: SPEC §10 Retroactive Console Application.
- [ ] 16.2 Implement the distinct user_id collection: query all distinct user_id values from artists, lyrics, lyric_reports, and likes tables. Ref: SPEC §7 Flow 12 step 3.
- [ ] 16.3 Implement username resolution: for each user_id, call the Cognito AdminGetUser API to resolve preferred_username. On failure, use fallback username "user-{first8charsOfUserId}". Ref: SPEC §7 Flow 12 step 4a.
- [ ] 16.4 Implement per-category point computation: artist_submission_points = SUM(CASE WHEN has_image THEN 5 ELSE 1 END) from non-deleted artists; artist_approval_points = SUM(CASE WHEN has_image THEN 10 ELSE 9 END) from approved non-deleted artists; lyric_submission_points = 5 * COUNT from non-deleted lyrics; lyric_approval_points = 15 * COUNT from approved non-deleted lyrics; report_submission_points = 1 * COUNT from non-deleted lyric_reports; report_acknowledgement_points = 4 * COUNT from acknowledged (status=1) non-deleted lyric_reports. Ref: SPEC §7 Flow 12 step 4b.
- [ ] 16.5 Implement like-based points computation: floor(COUNT(*) FROM likes WHERE user_id = X / 10). Ref: SPEC §7 Flow 12 step 4c.
- [ ] 16.6 Implement UPSERT into users table: INSERT ON CONFLICT (cognito_user_id) UPDATE, set all category columns and last_seen_points = total (prevents false indicator on first login). No rows inserted into point_events. Ref: SPEC §7 Flow 12 steps 4e, 5.
- [ ] 16.7 Implement progress logging: log each user processed, final summary (total users processed, total points awarded), and any errors to stdout. Exit code 0 on success, non-zero on failure. Ref: SPEC §7 Flow 12 step 6, §10 Retroactive Console Application.
- [ ] 16.8 Implement point_events detection: at startup, check if point_events table has any rows. If so, log a warning. If running post-launch, sum existing point_events per user and add to retroactive totals to avoid overwriting. Ref: SPEC §7 Flow 12 Idempotency.

## 17. Observability & Logging

- [x] 17.1 Add INFO-level logging for point event creation: log user ID, action type, entity ID, and points awarded. Ref: SPEC §8 Observability.
- [x] 17.2 Add INFO-level logging for daily limit hits: log user ID, action type, current count, and configured limit. Ref: SPEC §8 Observability.
- [x] 17.3 Add ERROR-level logging for point awarding failures with full context (user ID, action type, entity ID, exception details). Ref: SPEC §8 Observability.

## 18. Error Handling & Edge Cases

- [x] 18.1 Implement transaction rollback on point event insertion failure: if the INSERT into point_events or UPDATE of users fails, the transaction rolls back. The submission itself (already saved) is not affected. The user sees a generic success message (not the points message). Log the error. Ref: SPEC §9 Point Event Insertion Failure.
- [x] 18.2 Implement idempotent handling for duplicate approval awards: use INSERT ON CONFLICT DO NOTHING for point_events, and only increment the user's category total if the insert actually created a row. Ref: SPEC §9 Duplicate Point Award Attempt.
- [x] 18.3 Implement fallback handling in GetSubmitterPointsAsync: when no user record exists for a submitter, return 0 points, "New Contributor" label, and resolve the username via ICognitoService.GetPreferredUsernameAsync. Ref: SPEC §9 User Record Does Not Exist for Submitter.

## 19. Service Tests

- [x] 19.1 Write unit tests for contributor label derivation: verify correct labels at boundaries (0, 49, 50, 199, 200, 499, 500, 1000). Ref: SPEC §5 Contributor Labels.
- [x] 19.2 Write unit tests for EnsureUserExistsAsync: verify new user creation, existing user with same username (no-op), existing user with changed username (update). Ref: SPEC §7 Flow 1.
- [x] 19.3 Write unit tests for AwardSubmissionPointsAsync: verify points awarded within daily limit, points skipped when over limit, correct category column incremented, PointEvent creation. Ref: SPEC §7 Flow 2 step 6.
- [x] 19.4 Write unit tests for AwardApprovalPointsAsync: verify points awarded on first call, duplicate call skipped (idempotent), correct category column incremented. Ref: SPEC §7 Flow 5.
- [x] 19.5 Write unit tests for GetNavBarDataAsync: verify total computation (6 categories + like points), contributor label, HasPointsChanged flag. Ref: SPEC §7 Flow 10.
- [x] 19.6 Write unit tests for GetOwnProfileDataAsync: verify per-category breakdown, like points, total, activity feed limited to 20, LastSeenPoints update. Ref: SPEC §7 Flow 8.
- [x] 19.7 Write unit tests for GetPublicProfileDataAsync: verify total, label, contribution counts, null return for unknown username. Ref: SPEC §7 Flow 9.
- [x] 19.8 Write unit tests for GetSubmitterPointsAsync: verify normal return, fallback for missing user record. Ref: SPEC §7 Flow 11.
- [x] 19.9 Write unit tests for GetDailySubmissionCountAsync: verify correct count from source table for UTC day boundaries. Ref: SPEC §6 IBbPointsService.GetDailySubmissionCountAsync.
