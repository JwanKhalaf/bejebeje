namespace Bejebeje.Services.Tests.Services
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.DataAccess.Context;
  using Bejebeje.Domain;
  using Bejebeje.Services.Config;
  using Bejebeje.Services.Services;
  using Bejebeje.Services.Services.Interfaces;
  using Bejebeje.Services.Tests.Helpers;
  using FluentAssertions;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class BbPointsServiceEnsureUserExistsTests : DatabaseTestBase
  {
    private BbPointsService _service;
    private Mock<IOptions<BbPointsOptions>> _optionsMock;
    private Mock<IOptions<DatabaseOptions>> _dbOptionsMock;
    private Mock<ICognitoService> _cognitoServiceMock;
    private Mock<ILogger<BbPointsService>> _loggerMock;

    [SetUp]
    public void Setup()
    {
      SetupDataContext();

      _optionsMock = new Mock<IOptions<BbPointsOptions>>();
      _optionsMock.Setup(x => x.Value).Returns(new BbPointsOptions());

      _dbOptionsMock = new Mock<IOptions<DatabaseOptions>>();
      _dbOptionsMock.Setup(x => x.Value).Returns(new DatabaseOptions { ConnectionString = "unused" });

      _cognitoServiceMock = new Mock<ICognitoService>();
      _loggerMock = new Mock<ILogger<BbPointsService>>();

      _service = new BbPointsService(
        Context,
        _optionsMock.Object,
        _dbOptionsMock.Object,
        _cognitoServiceMock.Object,
        _loggerMock.Object);
    }

    [Test]
    public async Task should_create_new_user_when_not_found()
    {
      // act
      var result = await _service.EnsureUserExistsAsync("abc-123", "testuser");

      // assert
      result.Should().NotBeNull();
      result.CognitoUserId.Should().Be("abc-123");
      result.Username.Should().Be("testuser");
      result.ArtistSubmissionPoints.Should().Be(0);
      result.LastSeenPoints.Should().Be(0);
    }

    [Test]
    public async Task should_return_existing_user_when_username_matches()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "abc-123",
        Username = "testuser",
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      var result = await _service.EnsureUserExistsAsync("abc-123", "testuser");

      // assert
      result.Username.Should().Be("testuser");
      result.ModifiedAt.Should().BeNull();
      var count = await Context.Users.CountAsync();
      count.Should().Be(1);
    }

    [Test]
    public async Task should_update_username_when_different()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "abc-123",
        Username = "oldname",
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      var result = await _service.EnsureUserExistsAsync("abc-123", "newname");

      // assert
      result.Username.Should().Be("newname");
      result.ModifiedAt.Should().NotBeNull();
    }
  }

  [TestFixture]
  public class BbPointsServiceGetDailySubmissionCountTests : DatabaseTestBase
  {
    private BbPointsService _service;
    private Mock<IOptions<BbPointsOptions>> _optionsMock;
    private Mock<IOptions<DatabaseOptions>> _dbOptionsMock;
    private Mock<ICognitoService> _cognitoServiceMock;
    private Mock<ILogger<BbPointsService>> _loggerMock;

    [SetUp]
    public void Setup()
    {
      SetupDataContext();

      _optionsMock = new Mock<IOptions<BbPointsOptions>>();
      _optionsMock.Setup(x => x.Value).Returns(new BbPointsOptions());

      _dbOptionsMock = new Mock<IOptions<DatabaseOptions>>();
      _dbOptionsMock.Setup(x => x.Value).Returns(new DatabaseOptions { ConnectionString = "unused" });

      _cognitoServiceMock = new Mock<ICognitoService>();
      _loggerMock = new Mock<ILogger<BbPointsService>>();

      _service = new BbPointsService(
        Context,
        _optionsMock.Object,
        _dbOptionsMock.Object,
        _cognitoServiceMock.Object,
        _loggerMock.Object);
    }

    [Test]
    public async Task should_count_artist_submissions_for_today()
    {
      // arrange
      var today = DateTime.UtcNow;
      var yesterday = today.AddDays(-1);

      Context.Artists.Add(new Artist { UserId = "user-1", CreatedAt = today, FirstName = "A1", LastName = "L1", FullName = "A1 L1", Sex = 'M' });
      Context.Artists.Add(new Artist { UserId = "user-1", CreatedAt = today, FirstName = "A2", LastName = "L2", FullName = "A2 L2", Sex = 'M' });
      Context.Artists.Add(new Artist { UserId = "user-1", CreatedAt = yesterday, FirstName = "A3", LastName = "L3", FullName = "A3 L3", Sex = 'M' });
      Context.Artists.Add(new Artist { UserId = "user-2", CreatedAt = today, FirstName = "A4", LastName = "L4", FullName = "A4 L4", Sex = 'M' });
      await Context.SaveChangesAsync();

      // act
      var count = await _service.GetDailySubmissionCountAsync("user-1", PointActionType.ArtistSubmitted);

      // assert
      count.Should().Be(2);
    }

    [Test]
    public async Task should_count_lyric_submissions_for_today()
    {
      // arrange
      var today = DateTime.UtcNow;

      Context.Lyrics.Add(new Lyric { UserId = "user-1", CreatedAt = today, Title = "Song 1", Body = "Body", ArtistId = 0 });
      Context.Lyrics.Add(new Lyric { UserId = "user-1", CreatedAt = today, Title = "Song 2", Body = "Body", ArtistId = 0 });
      await Context.SaveChangesAsync();

      // act
      var count = await _service.GetDailySubmissionCountAsync("user-1", PointActionType.LyricSubmitted);

      // assert
      count.Should().Be(2);
    }

    [Test]
    public async Task should_count_report_submissions_for_today()
    {
      // arrange
      var today = DateTime.UtcNow;

      Context.LyricReports.Add(new LyricReport { UserId = "user-1", CreatedAt = today, LyricId = 1 });
      Context.LyricReports.Add(new LyricReport { UserId = "user-1", CreatedAt = today, LyricId = 2 });
      Context.LyricReports.Add(new LyricReport { UserId = "user-1", CreatedAt = today, LyricId = 3 });
      await Context.SaveChangesAsync();

      // act
      var count = await _service.GetDailySubmissionCountAsync("user-1", PointActionType.ReportSubmitted);

      // assert
      count.Should().Be(3);
    }

    [Test]
    public async Task should_return_zero_when_no_submissions_today()
    {
      // act
      var count = await _service.GetDailySubmissionCountAsync("user-1", PointActionType.ArtistSubmitted);

      // assert
      count.Should().Be(0);
    }
  }

  [TestFixture]
  public class BbPointsServiceAwardSubmissionPointsTests : DatabaseTestBase
  {
    private BbPointsService _service;
    private Mock<IOptions<BbPointsOptions>> _optionsMock;
    private Mock<IOptions<DatabaseOptions>> _dbOptionsMock;
    private Mock<ICognitoService> _cognitoServiceMock;
    private Mock<ILogger<BbPointsService>> _loggerMock;

    [SetUp]
    public void Setup()
    {
      SetupDataContext();

      _optionsMock = new Mock<IOptions<BbPointsOptions>>();
      _optionsMock.Setup(x => x.Value).Returns(new BbPointsOptions());

      _dbOptionsMock = new Mock<IOptions<DatabaseOptions>>();
      _dbOptionsMock.Setup(x => x.Value).Returns(new DatabaseOptions { ConnectionString = "unused" });

      _cognitoServiceMock = new Mock<ICognitoService>();
      _loggerMock = new Mock<ILogger<BbPointsService>>();

      _service = new BbPointsService(
        Context,
        _optionsMock.Object,
        _dbOptionsMock.Object,
        _cognitoServiceMock.Object,
        _loggerMock.Object);
    }

    [Test]
    public async Task should_award_points_when_within_daily_limit()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "user-1",
        Username = "testuser",
        CreatedAt = DateTime.UtcNow,
      });

      // add 2 artists today (limit is 5)
      Context.Artists.Add(new Artist { UserId = "user-1", CreatedAt = DateTime.UtcNow, FirstName = "A1", LastName = "L1", FullName = "A1 L1", Sex = 'M' });
      Context.Artists.Add(new Artist { UserId = "user-1", CreatedAt = DateTime.UtcNow, FirstName = "A2", LastName = "L2", FullName = "A2 L2", Sex = 'M' });
      await Context.SaveChangesAsync();

      // act
      var result = await _service.AwardSubmissionPointsAsync("user-1", "testuser", PointActionType.ArtistSubmitted, 99, "New Artist", 5);

      // assert
      result.Should().BeTrue();

      var pointEvent = await Context.PointEvents.FirstOrDefaultAsync();
      pointEvent.Should().NotBeNull();
      pointEvent.Points.Should().Be(5);
      pointEvent.ActionType.Should().Be(PointActionType.ArtistSubmitted);

      var user = await Context.Users.FirstAsync(u => u.CognitoUserId == "user-1");
      user.ArtistSubmissionPoints.Should().Be(5);
    }

    [Test]
    public async Task should_skip_points_when_over_daily_limit()
    {
      // arrange
      var options = new BbPointsOptions();
      options.DailyLimits.ArtistSubmissions = 2;
      _optionsMock.Setup(x => x.Value).Returns(options);

      _service = new BbPointsService(
        Context,
        _optionsMock.Object,
        _dbOptionsMock.Object,
        _cognitoServiceMock.Object,
        _loggerMock.Object);

      Context.Users.Add(new User
      {
        CognitoUserId = "user-1",
        Username = "testuser",
        CreatedAt = DateTime.UtcNow,
      });

      // add 2 artists today (limit is 2)
      Context.Artists.Add(new Artist { UserId = "user-1", CreatedAt = DateTime.UtcNow, FirstName = "A1", LastName = "L1", FullName = "A1 L1", Sex = 'M' });
      Context.Artists.Add(new Artist { UserId = "user-1", CreatedAt = DateTime.UtcNow, FirstName = "A2", LastName = "L2", FullName = "A2 L2", Sex = 'M' });
      await Context.SaveChangesAsync();

      // act
      var result = await _service.AwardSubmissionPointsAsync("user-1", "testuser", PointActionType.ArtistSubmitted, 99, "New Artist", 1);

      // assert
      result.Should().BeFalse();

      var pointEvents = await Context.PointEvents.CountAsync();
      pointEvents.Should().Be(0);

      var user = await Context.Users.FirstAsync(u => u.CognitoUserId == "user-1");
      user.ArtistSubmissionPoints.Should().Be(0);
    }

    [Test]
    public async Task should_create_point_event_with_correct_fields()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "user-1",
        Username = "testuser",
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      await _service.AwardSubmissionPointsAsync("user-1", "testuser", PointActionType.LyricSubmitted, 42, "Song Title", 5);

      // assert
      var pointEvent = await Context.PointEvents.FirstAsync();
      pointEvent.ActionType.Should().Be(PointActionType.LyricSubmitted);
      pointEvent.EntityId.Should().Be(42);
      pointEvent.EntityName.Should().Be("Song Title");
      pointEvent.Points.Should().Be(5);
    }

    [Test]
    public async Task should_increment_correct_category_for_lyric_submission()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "user-1",
        Username = "testuser",
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      await _service.AwardSubmissionPointsAsync("user-1", "testuser", PointActionType.LyricSubmitted, 1, "Song", 5);

      // assert
      var user = await Context.Users.FirstAsync(u => u.CognitoUserId == "user-1");
      user.LyricSubmissionPoints.Should().Be(5);
      user.ArtistSubmissionPoints.Should().Be(0);
    }

    [Test]
    public async Task should_increment_correct_category_for_report_submission()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "user-1",
        Username = "testuser",
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      await _service.AwardSubmissionPointsAsync("user-1", "testuser", PointActionType.ReportSubmitted, 1, "Report", 1);

      // assert
      var user = await Context.Users.FirstAsync(u => u.CognitoUserId == "user-1");
      user.ReportSubmissionPoints.Should().Be(1);
      user.ArtistSubmissionPoints.Should().Be(0);
      user.LyricSubmissionPoints.Should().Be(0);
    }

    [Test]
    public async Task should_create_user_if_not_exists_when_awarding()
    {
      // act
      var result = await _service.AwardSubmissionPointsAsync("new-user", "newuser", PointActionType.ArtistSubmitted, 1, "Artist", 1);

      // assert
      result.Should().BeTrue();
      var user = await Context.Users.FirstOrDefaultAsync(u => u.CognitoUserId == "new-user");
      user.Should().NotBeNull();
      user.Username.Should().Be("newuser");
    }
  }

  [TestFixture]
  public class BbPointsServiceAwardApprovalPointsTests : DatabaseTestBase
  {
    private BbPointsService _service;
    private Mock<IOptions<BbPointsOptions>> _optionsMock;
    private Mock<IOptions<DatabaseOptions>> _dbOptionsMock;
    private Mock<ICognitoService> _cognitoServiceMock;
    private Mock<ILogger<BbPointsService>> _loggerMock;

    [SetUp]
    public void Setup()
    {
      SetupDataContext();

      _optionsMock = new Mock<IOptions<BbPointsOptions>>();
      _optionsMock.Setup(x => x.Value).Returns(new BbPointsOptions());

      _dbOptionsMock = new Mock<IOptions<DatabaseOptions>>();
      _dbOptionsMock.Setup(x => x.Value).Returns(new DatabaseOptions { ConnectionString = "unused" });

      _cognitoServiceMock = new Mock<ICognitoService>();
      _loggerMock = new Mock<ILogger<BbPointsService>>();

      _service = new BbPointsService(
        Context,
        _optionsMock.Object,
        _dbOptionsMock.Object,
        _cognitoServiceMock.Object,
        _loggerMock.Object);
    }

    [Test]
    public async Task should_award_approval_points_first_time()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "user-1",
        Username = "testuser",
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      await _service.AwardApprovalPointsAsync("user-1", "testuser", PointActionType.ArtistApproved, 42, "Artist Name", 10);

      // assert
      var pointEvent = await Context.PointEvents.FirstOrDefaultAsync();
      pointEvent.Should().NotBeNull();
      pointEvent.Points.Should().Be(10);
      pointEvent.ActionType.Should().Be(PointActionType.ArtistApproved);

      var user = await Context.Users.FirstAsync(u => u.CognitoUserId == "user-1");
      user.ArtistApprovalPoints.Should().Be(10);
    }

    [Test]
    public async Task should_skip_duplicate_approval_award()
    {
      // arrange
      var user = new User
      {
        CognitoUserId = "user-1",
        Username = "testuser",
        CreatedAt = DateTime.UtcNow,
      };
      Context.Users.Add(user);
      await Context.SaveChangesAsync();

      Context.PointEvents.Add(new PointEvent
      {
        UserId = user.Id,
        ActionType = PointActionType.ArtistApproved,
        EntityId = 42,
        EntityName = "Artist Name",
        Points = 10,
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      user.ArtistApprovalPoints = 10;
      await Context.SaveChangesAsync();

      // act
      await _service.AwardApprovalPointsAsync("user-1", "testuser", PointActionType.ArtistApproved, 42, "Artist Name", 10);

      // assert
      var pointEvents = await Context.PointEvents.CountAsync();
      pointEvents.Should().Be(1);

      var updatedUser = await Context.Users.FirstAsync(u => u.CognitoUserId == "user-1");
      updatedUser.ArtistApprovalPoints.Should().Be(10);
    }

    [Test]
    public async Task should_increment_lyric_approval_points()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "user-1",
        Username = "testuser",
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      await _service.AwardApprovalPointsAsync("user-1", "testuser", PointActionType.LyricApproved, 1, "Song", 15);

      // assert
      var user = await Context.Users.FirstAsync(u => u.CognitoUserId == "user-1");
      user.LyricApprovalPoints.Should().Be(15);
    }

    [Test]
    public async Task should_increment_report_acknowledgement_points()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "user-1",
        Username = "testuser",
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      await _service.AwardApprovalPointsAsync("user-1", "testuser", PointActionType.ReportAcknowledged, 1, "Report", 4);

      // assert
      var user = await Context.Users.FirstAsync(u => u.CognitoUserId == "user-1");
      user.ReportAcknowledgementPoints.Should().Be(4);
    }

    [Test]
    public async Task should_create_user_if_not_exists_when_awarding_approval()
    {
      // act
      await _service.AwardApprovalPointsAsync("new-user", "newuser", PointActionType.ArtistApproved, 1, "Artist", 10);

      // assert
      var user = await Context.Users.FirstOrDefaultAsync(u => u.CognitoUserId == "new-user");
      user.Should().NotBeNull();
      user.ArtistApprovalPoints.Should().Be(10);
    }
  }
}
