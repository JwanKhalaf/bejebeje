namespace Bejebeje.Services.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bejebeje.Common.Extensions;
using Bejebeje.DataAccess.Context;
using Bejebeje.Shared.Domain;
using Bejebeje.Models.BbPoints;
using Bejebeje.Services.Config;
using Bejebeje.Services.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

public class BbPointsService : IBbPointsService
{
  private readonly BbContext _context;
  private readonly BbPointsOptions _options;
  private readonly DatabaseOptions _databaseOptions;
  private readonly ICognitoService _cognitoService;
  private readonly ILogger<BbPointsService> _logger;

  public BbPointsService(
    BbContext context,
    IOptions<BbPointsOptions> options,
    IOptions<DatabaseOptions> databaseOptions,
    ICognitoService cognitoService,
    ILogger<BbPointsService> logger)
  {
    _context = context;
    _options = options.Value;
    _databaseOptions = databaseOptions.Value;
    _cognitoService = cognitoService;
    _logger = logger;
  }

  public async Task<User> EnsureUserExistsAsync(string cognitoUserId, string username)
  {
    _logger.LogDebug("ensuring user exists for cognito id {CognitoUserId}", cognitoUserId);

    var trimmedUsername = username.Trim();

    var user = await _context.Users.FirstOrDefaultAsync(u => u.CognitoUserId == cognitoUserId);

    if (user == null)
    {
      // check for duplicate trimmed username before creating
      bool duplicateExists = await _context.Users
        .AnyAsync(u => u.Username == trimmedUsername && u.CognitoUserId != cognitoUserId);

      if (duplicateExists)
      {
        _logger.LogWarning(
          "duplicate username detected on new user creation: trimmed username {Username} already exists for a different user. cognito id {CognitoUserId}",
          trimmedUsername, cognitoUserId);
        return null;
      }

      var slug = await GenerateUniqueSlugAsync(trimmedUsername, cognitoUserId, currentUserId: null);

      _logger.LogDebug("generated slug {Slug} for new user {CognitoUserId}", slug, cognitoUserId);
      _logger.LogInformation("creating new user record for cognito id {CognitoUserId} with username {Username}", cognitoUserId, trimmedUsername);

      user = new User
      {
        CognitoUserId = cognitoUserId,
        Username = trimmedUsername,
        Slug = slug,
        CreatedAt = DateTime.UtcNow,
      };

      _context.Users.Add(user);
      await _context.SaveChangesAsync();

      return user;
    }

    if (user.Username != trimmedUsername)
    {
      // check for duplicate trimmed username before updating
      bool duplicateExists = await _context.Users
        .AnyAsync(u => u.Username == trimmedUsername && u.CognitoUserId != cognitoUserId);

      if (duplicateExists)
      {
        _logger.LogWarning(
          "duplicate username detected on username change: trimmed username {Username} already exists for a different user. cognito id {CognitoUserId}",
          trimmedUsername, cognitoUserId);
        return user;
      }

      var slug = await GenerateUniqueSlugAsync(trimmedUsername, cognitoUserId, currentUserId: user.Id);

      _logger.LogInformation(
        "updating username for cognito id {CognitoUserId} from {OldUsername} to {NewUsername}, slug changed to {Slug}",
        cognitoUserId, user.Username, trimmedUsername, slug);

      user.Username = trimmedUsername;
      user.Slug = slug;
      user.ModifiedAt = DateTime.UtcNow;
      await _context.SaveChangesAsync();
    }

    return user;
  }

  private async Task<string> GenerateUniqueSlugAsync(string trimmedUsername, string cognitoUserId, int? currentUserId)
  {
    var baseSlug = trimmedUsername.NormalizeStringForUrl();

    if (string.IsNullOrEmpty(baseSlug))
    {
      baseSlug = $"user-{cognitoUserId[..Math.Min(8, cognitoUserId.Length)]}";
      _logger.LogWarning("empty slug fallback triggered for cognito id {CognitoUserId}, using {BaseSlug}", cognitoUserId, baseSlug);
    }

    var candidateSlug = baseSlug;
    int suffix = 2;

    while (await _context.Users.AnyAsync(u => u.Slug == candidateSlug && u.Id != (currentUserId ?? 0)))
    {
      candidateSlug = $"{baseSlug}-{suffix}";
      suffix++;
    }

    return candidateSlug;
  }

  public async Task<int> GetDailySubmissionCountAsync(string cognitoUserId, PointActionType actionType)
  {
    var utcToday = DateTime.UtcNow.Date;
    var utcTomorrow = utcToday.AddDays(1);

    _logger.LogDebug("checking daily submission count for user {CognitoUserId}, action {ActionType}", cognitoUserId, actionType);

    int count = actionType switch
    {
      PointActionType.ArtistSubmitted => await _context.Artists
        .CountAsync(a => a.UserId == cognitoUserId && a.CreatedAt >= utcToday && a.CreatedAt < utcTomorrow),

      PointActionType.LyricSubmitted => await _context.Lyrics
        .CountAsync(l => l.UserId == cognitoUserId && l.CreatedAt >= utcToday && l.CreatedAt < utcTomorrow),

      PointActionType.ReportSubmitted => await _context.LyricReports
        .CountAsync(r => r.UserId == cognitoUserId && r.CreatedAt >= utcToday && r.CreatedAt < utcTomorrow),

      _ => 0,
    };

    _logger.LogDebug("user {CognitoUserId} has {Count} submissions today for action {ActionType}", cognitoUserId, count, actionType);

    return count;
  }

  public async Task<bool> AwardSubmissionPointsAsync(
    string cognitoUserId,
    string username,
    PointActionType actionType,
    int entityId,
    string entityName,
    int points)
  {
    _logger.LogDebug("awarding submission points: user {CognitoUserId}, action {ActionType}, entity {EntityId}, points {Points}", cognitoUserId, actionType, entityId, points);

    var user = await EnsureUserExistsAsync(cognitoUserId, username);

    int dailyCount = await GetDailySubmissionCountAsync(cognitoUserId, actionType);
    int dailyLimit = GetDailyLimit(actionType);

    if (dailyCount >= dailyLimit)
    {
      _logger.LogInformation("daily limit exceeded for user {CognitoUserId}, action {ActionType}: count {Count}, limit {Limit}", cognitoUserId, actionType, dailyCount, dailyLimit);
      return false;
    }

    var pointEvent = new PointEvent
    {
      UserId = user.Id,
      ActionType = actionType,
      Points = points,
      EntityId = entityId,
      EntityName = entityName,
      CreatedAt = DateTime.UtcNow,
    };

    _context.PointEvents.Add(pointEvent);
    IncrementCategoryPoints(user, actionType, points);
    await _context.SaveChangesAsync();

    _logger.LogInformation("awarded {Points} points to user {CognitoUserId} for {ActionType} on entity {EntityId}", points, cognitoUserId, actionType, entityId);

    return true;
  }

  public async Task AwardApprovalPointsAsync(
    string cognitoUserId,
    string username,
    PointActionType actionType,
    int entityId,
    string entityName,
    int points)
  {
    _logger.LogDebug("awarding approval points: user {CognitoUserId}, action {ActionType}, entity {EntityId}, points {Points}", cognitoUserId, actionType, entityId, points);

    var user = await EnsureUserExistsAsync(cognitoUserId, username);

    // check for duplicate
    bool alreadyAwarded = await _context.PointEvents.AnyAsync(pe =>
      pe.UserId == user.Id &&
      pe.ActionType == actionType &&
      pe.EntityId == entityId);

    if (alreadyAwarded)
    {
      _logger.LogInformation("duplicate approval award skipped for user {CognitoUserId}, action {ActionType}, entity {EntityId}", cognitoUserId, actionType, entityId);
      return;
    }

    var pointEvent = new PointEvent
    {
      UserId = user.Id,
      ActionType = actionType,
      Points = points,
      EntityId = entityId,
      EntityName = entityName,
      CreatedAt = DateTime.UtcNow,
    };

    _context.PointEvents.Add(pointEvent);
    IncrementCategoryPoints(user, actionType, points);
    await _context.SaveChangesAsync();

    _logger.LogInformation("awarded {Points} approval points to user {CognitoUserId} for {ActionType} on entity {EntityId}", points, cognitoUserId, actionType, entityId);
  }

  public async Task<NavBarPointsViewModel> GetNavBarDataAsync(string cognitoUserId)
  {
    _logger.LogDebug("fetching nav bar data for user {CognitoUserId}", cognitoUserId);

    var user = await _context.Users.FirstOrDefaultAsync(u => u.CognitoUserId == cognitoUserId);

    if (user == null)
    {
      return new NavBarPointsViewModel
      {
        TotalPoints = 0,
        ContributorLabel = BbPointsConstants.GetContributorLabel(0),
        HasPointsChanged = false,
      };
    }

    int categoryTotal = GetCategoryTotal(user);
    int likePoints = await GetLikePointsAsync(cognitoUserId);
    int totalPoints = categoryTotal + likePoints;

    return new NavBarPointsViewModel
    {
      TotalPoints = totalPoints,
      ContributorLabel = BbPointsConstants.GetContributorLabel(totalPoints),
      HasPointsChanged = totalPoints > user.LastSeenPoints,
    };
  }

  public async Task<OwnProfileViewModel> GetOwnProfileDataAsync(string cognitoUserId)
  {
    _logger.LogDebug("fetching own profile data for user {CognitoUserId}", cognitoUserId);

    var user = await _context.Users.FirstOrDefaultAsync(u => u.CognitoUserId == cognitoUserId);

    if (user == null)
    {
      return new OwnProfileViewModel
      {
        ContributorLabel = BbPointsConstants.GetContributorLabel(0),
      };
    }

    int likePoints = await GetLikePointsAsync(cognitoUserId);
    int totalPoints = GetCategoryTotal(user) + likePoints;

    var recentActivity = await _context.PointEvents
      .Where(pe => pe.UserId == user.Id)
      .OrderByDescending(pe => pe.CreatedAt)
      .Take(20)
      .Select(pe => new PointActivityViewModel
      {
        ActionDescription = GetActionDescription(pe.ActionType),
        EntityName = pe.EntityName,
        Points = pe.Points,
        Date = pe.CreatedAt,
      })
      .ToListAsync();

    // update last seen points
    user.LastSeenPoints = totalPoints;
    user.ModifiedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    _logger.LogInformation("own profile viewed by user {CognitoUserId}, total points {TotalPoints}", cognitoUserId, totalPoints);

    return new OwnProfileViewModel
    {
      ArtistSubmissionPoints = user.ArtistSubmissionPoints,
      ArtistApprovalPoints = user.ArtistApprovalPoints,
      LyricSubmissionPoints = user.LyricSubmissionPoints,
      LyricApprovalPoints = user.LyricApprovalPoints,
      ReportSubmissionPoints = user.ReportSubmissionPoints,
      ReportAcknowledgementPoints = user.ReportAcknowledgementPoints,
      LikePoints = likePoints,
      TotalPoints = totalPoints,
      ContributorLabel = BbPointsConstants.GetContributorLabel(totalPoints),
      Username = user.Username,
      RecentActivity = recentActivity,
    };
  }

  public async Task<PublicProfileViewModel> GetPublicProfileDataAsync(string slug)
  {
    _logger.LogDebug("fetching public profile data for slug {Slug}", slug);

    var user = await _context.Users.FirstOrDefaultAsync(u => u.Slug == slug);

    if (user == null)
    {
      _logger.LogDebug("no user found for slug {Slug}", slug);
      return null;
    }

    int likePoints = await GetLikePointsAsync(user.CognitoUserId);
    int totalPoints = GetCategoryTotal(user) + likePoints;

    int artistsSubmittedCount = await _context.Artists
      .CountAsync(a => a.UserId == user.CognitoUserId && !a.IsDeleted);

    int lyricsSubmittedCount = await _context.Lyrics
      .CountAsync(l => l.UserId == user.CognitoUserId && !l.IsDeleted);

    return new PublicProfileViewModel
    {
      Username = user.Username,
      CognitoUserId = user.CognitoUserId,
      TotalPoints = totalPoints,
      ContributorLabel = BbPointsConstants.GetContributorLabel(totalPoints),
      ArtistsSubmittedCount = artistsSubmittedCount,
      LyricsSubmittedCount = lyricsSubmittedCount,
    };
  }

  public async Task<SubmitterPointsViewModel> GetSubmitterPointsAsync(string cognitoUserId)
  {
    _logger.LogDebug("fetching submitter points for cognito id {CognitoUserId}", cognitoUserId);

    var user = await _context.Users.FirstOrDefaultAsync(u => u.CognitoUserId == cognitoUserId);

    if (user != null)
    {
      int likePoints = await GetLikePointsAsync(cognitoUserId);
      int totalPoints = GetCategoryTotal(user) + likePoints;

      return new SubmitterPointsViewModel
      {
        TotalPoints = totalPoints,
        ContributorLabel = BbPointsConstants.GetContributorLabel(totalPoints),
        Username = user.Username,
        Slug = user.Slug,
      };
    }

    // fallback for users without a local record
    _logger.LogDebug("no user record for cognito id {CognitoUserId}, resolving username from cognito", cognitoUserId);

    string resolvedUsername;
    try
    {
      resolvedUsername = await _cognitoService.GetPreferredUsernameAsync(cognitoUserId);
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "failed to resolve username from cognito for {CognitoUserId}", cognitoUserId);
      resolvedUsername = $"user-{cognitoUserId[..Math.Min(8, cognitoUserId.Length)]}";
    }

    return new SubmitterPointsViewModel
    {
      TotalPoints = 0,
      ContributorLabel = BbPointsConstants.GetContributorLabel(0),
      Username = resolvedUsername,
      Slug = null,
    };
  }

  private int GetDailyLimit(PointActionType actionType)
  {
    return actionType switch
    {
      PointActionType.ArtistSubmitted => _options.DailyLimits.ArtistSubmissions,
      PointActionType.LyricSubmitted => _options.DailyLimits.LyricSubmissions,
      PointActionType.ReportSubmitted => _options.DailyLimits.ReportSubmissions,
      _ => 0,
    };
  }

  private static void IncrementCategoryPoints(User user, PointActionType actionType, int points)
  {
    switch (actionType)
    {
      case PointActionType.ArtistSubmitted:
        user.ArtistSubmissionPoints += points;
        break;
      case PointActionType.ArtistApproved:
        user.ArtistApprovalPoints += points;
        break;
      case PointActionType.LyricSubmitted:
        user.LyricSubmissionPoints += points;
        break;
      case PointActionType.LyricApproved:
        user.LyricApprovalPoints += points;
        break;
      case PointActionType.ReportSubmitted:
        user.ReportSubmissionPoints += points;
        break;
      case PointActionType.ReportAcknowledged:
        user.ReportAcknowledgementPoints += points;
        break;
    }
  }

  private static int GetCategoryTotal(User user)
  {
    return user.ArtistSubmissionPoints
      + user.ArtistApprovalPoints
      + user.LyricSubmissionPoints
      + user.LyricApprovalPoints
      + user.ReportSubmissionPoints
      + user.ReportAcknowledgementPoints;
  }

  private async Task<int> GetLikePointsAsync(string cognitoUserId)
  {
    // the likes table is not managed by ef core, so we use raw sql
    try
    {
      await using var connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
      await connection.OpenAsync();

      await using var command = new NpgsqlCommand(
        "select count(*) from likes where user_id = @user_id;",
        connection);
      command.Parameters.AddWithValue("@user_id", cognitoUserId);

      var result = await command.ExecuteScalarAsync();
      int likeCount = Convert.ToInt32(result);

      return likeCount / BbPointsConstants.LikesPerPoint;
    }
    catch (Exception ex)
    {
      // in test environments (in-memory db), the likes table won't exist
      _logger.LogDebug(ex, "could not query likes table for user {CognitoUserId}, returning 0 like points", cognitoUserId);
      return 0;
    }
  }

  private static string GetActionDescription(PointActionType actionType)
  {
    return actionType switch
    {
      PointActionType.ArtistSubmitted => "Submitted artist",
      PointActionType.ArtistApproved => "Artist approved",
      PointActionType.LyricSubmitted => "Submitted lyric",
      PointActionType.LyricApproved => "Lyric approved",
      PointActionType.ReportSubmitted => "Submitted report",
      PointActionType.ReportAcknowledged => "Report acknowledged",
      _ => "Unknown action",
    };
  }
}
