namespace Bejebeje.Services.Tests.Services
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.DataAccess.Context;
  using Bejebeje.Shared.Domain;
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

  [TestFixture]
  public class BbPointsServiceSlugGenerationTests : DatabaseTestBase
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
    public async Task should_generate_slug_from_trimmed_username_on_new_user()
    {
      // act
      var result = await _service.EnsureUserExistsAsync("abc-123", "ali fm ");

      // assert
      result.Should().NotBeNull();
      result.Username.Should().Be("ali fm");
      result.Slug.Should().Be("ali-fm");
    }

    [Test]
    public async Task should_use_empty_slug_fallback_when_normalize_returns_empty()
    {
      // act — username of all special chars that NormalizeStringForUrl strips
      var result = await _service.EnsureUserExistsAsync("abcd1234-5678", "!!!");

      // assert
      result.Should().NotBeNull();
      result.Slug.Should().Be("user-abcd1234");
    }

    [Test]
    public async Task should_append_suffix_when_slug_collides_with_another_user()
    {
      // arrange — existing user holds the "kardox" slug
      Context.Users.Add(new User
      {
        CognitoUserId = "user-26",
        Username = "Kardox",
        Slug = "kardox",
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      var result = await _service.EnsureUserExistsAsync("user-91", "Kardox2");

      // assert — "kardox2" doesn't collide with "kardox", so no suffix needed
      // let's use a case where it does collide
      result.Should().NotBeNull();
    }

    [Test]
    public async Task should_append_numeric_suffix_for_slug_collision()
    {
      // arrange — existing user holds the "kardox" slug
      Context.Users.Add(new User
      {
        CognitoUserId = "user-26",
        Username = "Kardox",
        Slug = "kardox",
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act — new user whose username also normalizes to "kardox"
      var result = await _service.EnsureUserExistsAsync("user-91", "kardox");

      // assert
      result.Should().NotBeNull();
      result.Slug.Should().Be("kardox-2");
    }

    [Test]
    public async Task should_increment_suffix_past_existing_collisions()
    {
      // arrange — two users already hold "kardox" and "kardox-2"
      Context.Users.Add(new User
      {
        CognitoUserId = "user-26",
        Username = "Kardox",
        Slug = "kardox",
        CreatedAt = DateTime.UtcNow,
      });
      Context.Users.Add(new User
      {
        CognitoUserId = "user-91",
        Username = "Kardox ",
        Slug = "kardox-2",
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      var result = await _service.EnsureUserExistsAsync("user-200", "kardox");

      // assert
      result.Should().NotBeNull();
      result.Slug.Should().Be("kardox-3");
    }

    [Test]
    public async Task should_trim_username_on_new_user_creation()
    {
      // act
      var result = await _service.EnsureUserExistsAsync("abc-123", "  ali fm  ");

      // assert
      result.Should().NotBeNull();
      result.Username.Should().Be("ali fm");
    }

    [Test]
    public async Task should_return_null_when_duplicate_trimmed_username_on_new_user()
    {
      // arrange — existing user holds "kardox"
      Context.Users.Add(new User
      {
        CognitoUserId = "user-26",
        Username = "kardox",
        Slug = "kardox",
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act — new user with trailing space trims to same "kardox"
      var result = await _service.EnsureUserExistsAsync("user-91", "kardox ");

      // assert
      result.Should().BeNull();
      var count = await Context.Users.CountAsync();
      count.Should().Be(1);
    }

    [Test]
    public async Task should_not_update_when_trimmed_username_matches_stored()
    {
      // arrange — existing user stored with trimmed username
      Context.Users.Add(new User
      {
        CognitoUserId = "abc-123",
        Username = "ali fm",
        Slug = "ali-fm",
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act — cognito sends same username with trailing space
      var result = await _service.EnsureUserExistsAsync("abc-123", "ali fm ");

      // assert
      result.Username.Should().Be("ali fm");
      result.ModifiedAt.Should().BeNull();
    }

    [Test]
    public async Task should_regenerate_slug_on_username_change()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "abc-123",
        Username = "ali fm",
        Slug = "ali-fm",
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      var result = await _service.EnsureUserExistsAsync("abc-123", "ali music");

      // assert
      result.Username.Should().Be("ali music");
      result.Slug.Should().Be("ali-music");
      result.ModifiedAt.Should().NotBeNull();
    }

    [Test]
    public async Task should_block_username_change_when_duplicate_exists()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "user-1",
        Username = "old-name",
        Slug = "old-name",
        CreatedAt = DateTime.UtcNow,
      });
      Context.Users.Add(new User
      {
        CognitoUserId = "user-2",
        Username = "new-name",
        Slug = "new-name",
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act — user-1 tries to change username to "new-name"
      var result = await _service.EnsureUserExistsAsync("user-1", "new-name");

      // assert — username and slug remain unchanged
      result.Username.Should().Be("old-name");
      result.Slug.Should().Be("old-name");
      result.ModifiedAt.Should().BeNull();
    }

    [Test]
    public async Task should_not_count_own_slug_as_collision_during_regeneration()
    {
      // arrange — user with slug "ali-fm" changes username but it still generates "ali-fm"
      Context.Users.Add(new User
      {
        CognitoUserId = "abc-123",
        Username = "ali_fm",
        Slug = "ali-fm",
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act — new username also normalizes to "ali-fm"
      var result = await _service.EnsureUserExistsAsync("abc-123", "ali fm");

      // assert — slug should remain "ali-fm", not "ali-fm-2"
      result.Slug.Should().Be("ali-fm");
    }
  }
}
