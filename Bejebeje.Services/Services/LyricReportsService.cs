namespace Bejebeje.Services.Services;

using System;
using System.Threading.Tasks;
using Bejebeje.Models.Report;
using Bejebeje.Services.Config;
using Bejebeje.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

public class LyricReportsService : ILyricReportsService
{
  private readonly DatabaseOptions _databaseOptions;
  private readonly IEmailService _emailService;
  private readonly ICognitoService _cognitoService;
  private readonly ILogger<LyricReportsService> _logger;

  public LyricReportsService(
    IOptionsMonitor<DatabaseOptions> optionsAccessor,
    IEmailService emailService,
    ICognitoService cognitoService,
    ILogger<LyricReportsService> logger)
  {
    _databaseOptions = optionsAccessor.CurrentValue;
    _emailService = emailService;
    _cognitoService = cognitoService;
    _logger = logger;
  }

  public async Task<int> GetReportCountForUserTodayAsync(string userId)
  {
    _logger.LogDebug("checking daily report count for user {UserId}", userId);

    string sql = "select count(*) from lyric_reports where user_id = @user_id and created_at >= @utc_today_start and is_deleted = false;";

    await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
    await connection.OpenAsync();

    await using NpgsqlCommand command = new NpgsqlCommand(sql, connection);
    command.Parameters.AddWithValue("@user_id", userId);
    command.Parameters.AddWithValue("@utc_today_start", DateTime.UtcNow.Date);

    object result = await command.ExecuteScalarAsync();
    int count = Convert.ToInt32(result);

    _logger.LogDebug("user {UserId} has {ReportCount} reports today", userId, count);

    return count;
  }

  public async Task<bool> HasPendingReportForLyricAsync(string userId, int lyricId)
  {
    _logger.LogDebug("checking for pending report by user {UserId} on lyric {LyricId}", userId, lyricId);

    string sql = "select exists(select 1 from lyric_reports where user_id = @user_id and lyric_id = @lyric_id and status = 0 and is_deleted = false);";

    await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
    await connection.OpenAsync();

    await using NpgsqlCommand command = new NpgsqlCommand(sql, connection);
    command.Parameters.AddWithValue("@user_id", userId);
    command.Parameters.AddWithValue("@lyric_id", lyricId);

    object result = await command.ExecuteScalarAsync();
    bool hasPending = Convert.ToBoolean(result);

    _logger.LogDebug("user {UserId} has pending report for lyric {LyricId}: {HasPending}", userId, lyricId, hasPending);

    return hasPending;
  }

  public async Task<LyricReportViewModel> GetLyricDetailsForReportAsync(string artistSlug, string lyricSlug)
  {
    _logger.LogDebug("fetching lyric details for report page: artist {ArtistSlug}, lyric {LyricSlug}", artistSlug, lyricSlug);

    string sql = @"
      select l.id, l.title, l.body,
             a.first_name, a.last_name,
             ars.name as artist_slug, ls.name as lyric_slug
      from lyrics l
      inner join artists a on l.artist_id = a.id
      inner join artist_slugs ars on ars.artist_id = a.id
      inner join lyric_slugs ls on ls.lyric_id = l.id
      where ars.name = @artist_slug
        and ls.name = @lyric_slug
        and l.is_approved = true
        and l.is_deleted = false
        and a.is_deleted = false
        and ars.is_primary = true
        and ls.is_primary = true;";

    await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
    await connection.OpenAsync();

    await using NpgsqlCommand command = new NpgsqlCommand(sql, connection);
    command.Parameters.AddWithValue("@artist_slug", artistSlug);
    command.Parameters.AddWithValue("@lyric_slug", lyricSlug);

    await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

    if (await reader.ReadAsync())
    {
      string firstName = Convert.ToString(reader["first_name"]);
      string lastName = Convert.ToString(reader["last_name"]);
      string artistName = $"{firstName} {lastName}".Trim();

      var viewModel = new LyricReportViewModel
      {
        LyricId = Convert.ToInt32(reader["id"]),
        LyricTitle = Convert.ToString(reader["title"]).Trim(),
        LyricBody = Convert.ToString(reader["body"]).Trim(),
        ArtistName = artistName,
        ArtistSlug = Convert.ToString(reader["artist_slug"]),
        LyricSlug = Convert.ToString(reader["lyric_slug"]),
      };

      _logger.LogDebug("found lyric {LyricId} for report page", viewModel.LyricId);

      return viewModel;
    }

    _logger.LogDebug("lyric not found or not approved for artist {ArtistSlug}, lyric {LyricSlug}", artistSlug, lyricSlug);

    return null;
  }

  public async Task<int> CreateReportAsync(string userId, int lyricId, int category, string comment)
  {
    _logger.LogInformation("creating lyric report: user {UserId}, lyric {LyricId}, category {Category}", userId, lyricId, category);

    string sql = @"
      insert into lyric_reports (lyric_id, user_id, category, comment, status, created_at, is_deleted)
      values (@lyric_id, @user_id, @category, @comment, 0, @created_at, false)
      returning id;";

    await using NpgsqlConnection connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
    await connection.OpenAsync();

    await using NpgsqlCommand command = new NpgsqlCommand(sql, connection);
    command.Parameters.AddWithValue("@lyric_id", lyricId);
    command.Parameters.AddWithValue("@user_id", userId);
    command.Parameters.AddWithValue("@category", category);
    command.Parameters.AddWithValue("@comment", (object)comment ?? DBNull.Value);
    command.Parameters.AddWithValue("@created_at", DateTime.UtcNow);

    object result = await command.ExecuteScalarAsync();
    int reportId = Convert.ToInt32(result);

    _logger.LogInformation("lyric report {ReportId} created successfully for lyric {LyricId}", reportId, lyricId);

    return reportId;
  }

  public async Task SendReportEmailsAsync(
    string userId,
    string reporterEmail,
    string lyricTitle,
    string artistName,
    string categoryDisplayLabel,
    string comment)
  {
    // fetch reporter username
    string reporterUsername;
    try
    {
      reporterUsername = await _cognitoService.GetPreferredUsernameAsync(userId);
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "failed to fetch username for user {UserId}, using fallback", userId);
      reporterUsername = "Unknown User";
    }

    // send admin notification email
    try
    {
      _logger.LogDebug("sending admin notification email for lyric report: {LyricTitle}", lyricTitle);
      await _emailService.SendLyricReportNotificationEmailAsync(reporterUsername, lyricTitle, artistName, categoryDisplayLabel, comment);
      _logger.LogInformation("admin notification email sent for lyric report: {LyricTitle}", lyricTitle);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "failed to send admin notification email for lyric report: {LyricTitle}", lyricTitle);
    }

    // send reporter confirmation email
    if (string.IsNullOrEmpty(reporterEmail))
    {
      _logger.LogWarning("reporter email claim is missing for user {UserId}, skipping confirmation email", userId);
      return;
    }

    try
    {
      _logger.LogDebug("sending confirmation email to reporter {ReporterEmail} for lyric report: {LyricTitle}", reporterEmail, lyricTitle);
      await _emailService.SendLyricReportConfirmationEmailAsync(reporterEmail, lyricTitle, artistName);
      _logger.LogInformation("confirmation email sent to reporter for lyric report: {LyricTitle}", lyricTitle);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "failed to send confirmation email to reporter for lyric report: {LyricTitle}", lyricTitle);
    }
  }
}
