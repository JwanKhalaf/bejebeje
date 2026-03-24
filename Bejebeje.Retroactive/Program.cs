using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Npgsql;

// resolve connection string from args or environment variable
string connectionString = args.Length > 0
  ? args[0]
  : Environment.GetEnvironmentVariable("CONNECTION_STRING");

if (string.IsNullOrEmpty(connectionString))
{
  Console.Error.WriteLine("error: connection string required. pass as first argument or set CONNECTION_STRING environment variable.");
  return 1;
}

// resolve cognito user pool id from args or environment variable
string userPoolId = args.Length > 1
  ? args[1]
  : Environment.GetEnvironmentVariable("COGNITO_USER_POOL_ID");

if (string.IsNullOrEmpty(userPoolId))
{
  Console.Error.WriteLine("error: cognito user pool id required. pass as second argument or set COGNITO_USER_POOL_ID environment variable.");
  return 1;
}

Console.WriteLine("bejebeje retroactive bb points calculation");
Console.WriteLine("==========================================");
Console.WriteLine($"started at: {DateTime.UtcNow:O}");

try
{
  await using var connection = new NpgsqlConnection(connectionString);
  await connection.OpenAsync();
  Console.WriteLine("connected to database");

  // task 16.8: check if point_events table has any rows
  bool hasExistingPointEvents = false;
  Dictionary<string, int> existingPointEventTotals = new();

  await using (var checkCmd = new NpgsqlCommand("select count(*) from point_events;", connection))
  {
    long count = (long)await checkCmd.ExecuteScalarAsync();

    if (count > 0)
    {
      hasExistingPointEvents = true;
      Console.WriteLine($"WARNING: point_events table has {count} existing rows. running in post-launch mode — will preserve existing point event totals.");

      // sum existing point_events per user (by cognito_user_id via users table)
      await using var sumCmd = new NpgsqlCommand(
        "select u.cognito_user_id, sum(pe.points) from point_events pe inner join users u on pe.user_id = u.id group by u.cognito_user_id;",
        connection);

      await using var sumReader = await sumCmd.ExecuteReaderAsync();

      while (await sumReader.ReadAsync())
      {
        string cognitoId = sumReader.GetString(0);
        int total = sumReader.GetInt32(1);
        existingPointEventTotals[cognitoId] = total;
      }

      Console.WriteLine($"found existing point event totals for {existingPointEventTotals.Count} users");
    }
    else
    {
      Console.WriteLine("point_events table is empty — running in pre-launch mode");
    }
  }

  // task 16.2: collect all distinct user_ids
  Console.WriteLine("collecting distinct user ids...");
  HashSet<string> userIds = new();

  string[] queries =
  [
    "select distinct user_id from artists where user_id is not null and user_id != '';",
    "select distinct user_id from lyrics where user_id is not null and user_id != '';",
    "select distinct user_id from lyric_reports where user_id is not null and user_id != '';",
    "select distinct user_id from likes where user_id is not null and user_id != '';",
  ];

  foreach (string query in queries)
  {
    await using var cmd = new NpgsqlCommand(query, connection);
    await using var reader = await cmd.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
      string userId = reader.GetString(0);

      if (!string.IsNullOrWhiteSpace(userId))
      {
        userIds.Add(userId);
      }
    }
  }

  Console.WriteLine($"found {userIds.Count} distinct user ids");

  // create cognito client for username resolution
  using var cognitoClient = new AmazonCognitoIdentityProviderClient();

  int usersProcessed = 0;
  int totalPointsAwarded = 0;
  int errors = 0;

  foreach (string userId in userIds)
  {
    try
    {
      // task 16.3: resolve username from cognito
      string username;

      try
      {
        var adminGetUserRequest = new AdminGetUserRequest
        {
          UserPoolId = userPoolId,
          Username = userId,
        };

        var response = await cognitoClient.AdminGetUserAsync(adminGetUserRequest);

        var preferredUsernameAttr = response.UserAttributes.Find(a => a.Name == "preferred_username");
        username = preferredUsernameAttr?.Value ?? $"user-{userId[..Math.Min(8, userId.Length)]}";
      }
      catch (UserNotFoundException)
      {
        username = $"user-{userId[..Math.Min(8, userId.Length)]}";
        Console.WriteLine($"  cognito user not found for {userId}, using fallback: {username}");
      }
      catch (Exception ex)
      {
        username = $"user-{userId[..Math.Min(8, userId.Length)]}";
        Console.WriteLine($"  cognito error for {userId}: {ex.Message}, using fallback: {username}");
      }

      // task 16.4: compute per-category points

      // artist submission points: SUM(CASE WHEN has_image THEN 5 ELSE 1 END) from non-deleted artists
      int artistSubmissionPoints = 0;
      await using (var cmd = new NpgsqlCommand(
        "select coalesce(sum(case when has_image then 5 else 1 end), 0) from artists where user_id = @uid and is_deleted = false;",
        connection))
      {
        cmd.Parameters.AddWithValue("@uid", userId);
        artistSubmissionPoints = Convert.ToInt32(await cmd.ExecuteScalarAsync());
      }

      // artist approval points: SUM(CASE WHEN has_image THEN 10 ELSE 9 END) from approved non-deleted artists
      int artistApprovalPoints = 0;
      await using (var cmd = new NpgsqlCommand(
        "select coalesce(sum(case when has_image then 10 else 9 end), 0) from artists where user_id = @uid and is_deleted = false and is_approved = true;",
        connection))
      {
        cmd.Parameters.AddWithValue("@uid", userId);
        artistApprovalPoints = Convert.ToInt32(await cmd.ExecuteScalarAsync());
      }

      // lyric submission points: 5 * COUNT from non-deleted lyrics
      int lyricSubmissionPoints = 0;
      await using (var cmd = new NpgsqlCommand(
        "select coalesce(count(*) * 5, 0) from lyrics where user_id = @uid and is_deleted = false;",
        connection))
      {
        cmd.Parameters.AddWithValue("@uid", userId);
        lyricSubmissionPoints = Convert.ToInt32(await cmd.ExecuteScalarAsync());
      }

      // lyric approval points: 15 * COUNT from approved non-deleted lyrics
      int lyricApprovalPoints = 0;
      await using (var cmd = new NpgsqlCommand(
        "select coalesce(count(*) * 15, 0) from lyrics where user_id = @uid and is_deleted = false and is_approved = true;",
        connection))
      {
        cmd.Parameters.AddWithValue("@uid", userId);
        lyricApprovalPoints = Convert.ToInt32(await cmd.ExecuteScalarAsync());
      }

      // report submission points: 1 * COUNT from non-deleted lyric_reports
      int reportSubmissionPoints = 0;
      await using (var cmd = new NpgsqlCommand(
        "select coalesce(count(*), 0) from lyric_reports where user_id = @uid and is_deleted = false;",
        connection))
      {
        cmd.Parameters.AddWithValue("@uid", userId);
        reportSubmissionPoints = Convert.ToInt32(await cmd.ExecuteScalarAsync());
      }

      // report acknowledgement points: 4 * COUNT from acknowledged (status=1) non-deleted lyric_reports
      int reportAcknowledgementPoints = 0;
      await using (var cmd = new NpgsqlCommand(
        "select coalesce(count(*) * 4, 0) from lyric_reports where user_id = @uid and is_deleted = false and status = 1;",
        connection))
      {
        cmd.Parameters.AddWithValue("@uid", userId);
        reportAcknowledgementPoints = Convert.ToInt32(await cmd.ExecuteScalarAsync());
      }

      // task 16.5: like-based points: floor(COUNT(*) / 10)
      int likePoints = 0;
      await using (var cmd = new NpgsqlCommand(
        "select coalesce(floor(count(*) / 10), 0) from likes where user_id = @uid;",
        connection))
      {
        cmd.Parameters.AddWithValue("@uid", userId);
        likePoints = Convert.ToInt32(await cmd.ExecuteScalarAsync());
      }

      int totalPoints = artistSubmissionPoints + artistApprovalPoints
        + lyricSubmissionPoints + lyricApprovalPoints
        + reportSubmissionPoints + reportAcknowledgementPoints
        + likePoints;

      // task 16.6: upsert into users table
      // last_seen_points = total prevents false points-changed indicator on first login
      int lastSeenPoints = totalPoints;

      await using (var cmd = new NpgsqlCommand(@"
        insert into users (cognito_user_id, username, artist_submission_points, artist_approval_points,
          lyric_submission_points, lyric_approval_points, report_submission_points, report_acknowledgement_points,
          last_seen_points, created_at)
        values (@cognito_user_id, @username, @asp, @aap, @lsp, @lap, @rsp, @rap, @lsp_total, @created_at)
        on conflict (cognito_user_id) do update set
          username = @username,
          artist_submission_points = @asp,
          artist_approval_points = @aap,
          lyric_submission_points = @lsp,
          lyric_approval_points = @lap,
          report_submission_points = @rsp,
          report_acknowledgement_points = @rap,
          last_seen_points = @lsp_total,
          modified_at = @modified_at;",
        connection))
      {
        cmd.Parameters.AddWithValue("@cognito_user_id", userId);
        cmd.Parameters.AddWithValue("@username", username);
        cmd.Parameters.AddWithValue("@asp", artistSubmissionPoints);
        cmd.Parameters.AddWithValue("@aap", artistApprovalPoints);
        cmd.Parameters.AddWithValue("@lsp", lyricSubmissionPoints);
        cmd.Parameters.AddWithValue("@lap", lyricApprovalPoints);
        cmd.Parameters.AddWithValue("@rsp", reportSubmissionPoints);
        cmd.Parameters.AddWithValue("@rap", reportAcknowledgementPoints);
        cmd.Parameters.AddWithValue("@lsp_total", lastSeenPoints);
        cmd.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("@modified_at", DateTime.UtcNow);

        await cmd.ExecuteNonQueryAsync();
      }

      usersProcessed++;
      totalPointsAwarded += totalPoints;

      Console.WriteLine($"  [{usersProcessed}/{userIds.Count}] {username} ({userId[..Math.Min(8, userId.Length)]}...): {totalPoints} points (a:{artistSubmissionPoints}+{artistApprovalPoints} l:{lyricSubmissionPoints}+{lyricApprovalPoints} r:{reportSubmissionPoints}+{reportAcknowledgementPoints} likes:{likePoints})");
    }
    catch (Exception ex)
    {
      errors++;
      Console.Error.WriteLine($"  ERROR processing user {userId}: {ex.Message}");
    }
  }

  // task 16.7: final summary
  Console.WriteLine();
  Console.WriteLine("==========================================");
  Console.WriteLine("retroactive calculation complete");
  Console.WriteLine($"  users processed: {usersProcessed}");
  Console.WriteLine($"  total points awarded: {totalPointsAwarded}");
  Console.WriteLine($"  errors: {errors}");
  Console.WriteLine($"  mode: {(hasExistingPointEvents ? "post-launch" : "pre-launch")}");
  Console.WriteLine($"  completed at: {DateTime.UtcNow:O}");

  if (errors > 0)
  {
    Console.Error.WriteLine($"warning: {errors} errors occurred during processing");
  }

  return errors > 0 ? 1 : 0;
}
catch (Exception ex)
{
  Console.Error.WriteLine($"fatal error: {ex.Message}");
  Console.Error.WriteLine(ex.StackTrace);
  return 1;
}
