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
  public class BbPointsServiceGetNavBarDataTests : DatabaseTestBase
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
    public async Task should_return_total_from_six_categories()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "user-1",
        Username = "testuser",
        ArtistSubmissionPoints = 5,
        LyricSubmissionPoints = 10,
        ArtistApprovalPoints = 9,
        LyricApprovalPoints = 15,
        ReportSubmissionPoints = 1,
        ReportAcknowledgementPoints = 4,
        LastSeenPoints = 0,
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetNavBarDataAsync("user-1");

      // assert (like points = 0 in test since likes table doesn't exist)
      result.TotalPoints.Should().Be(44);
      result.ContributorLabel.Should().Be("New Contributor");
    }

    [Test]
    public async Task should_set_has_points_changed_when_total_exceeds_last_seen()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "user-1",
        Username = "testuser",
        ArtistSubmissionPoints = 50,
        LastSeenPoints = 0,
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetNavBarDataAsync("user-1");

      // assert
      result.HasPointsChanged.Should().BeTrue();
    }

    [Test]
    public async Task should_not_set_has_points_changed_when_equal_to_last_seen()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "user-1",
        Username = "testuser",
        ArtistSubmissionPoints = 50,
        LastSeenPoints = 50,
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetNavBarDataAsync("user-1");

      // assert
      result.HasPointsChanged.Should().BeFalse();
    }

    [Test]
    public async Task should_return_zero_points_for_unknown_user()
    {
      // act
      var result = await _service.GetNavBarDataAsync("nonexistent");

      // assert
      result.TotalPoints.Should().Be(0);
      result.ContributorLabel.Should().Be("New Contributor");
      result.HasPointsChanged.Should().BeFalse();
    }

    [Test]
    public async Task should_derive_correct_contributor_label()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "user-1",
        Username = "testuser",
        ArtistSubmissionPoints = 200,
        LastSeenPoints = 200,
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetNavBarDataAsync("user-1");

      // assert
      result.ContributorLabel.Should().Be("Regular Contributor");
    }
  }

  [TestFixture]
  public class BbPointsServiceGetOwnProfileDataTests : DatabaseTestBase
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
    public async Task should_return_per_category_breakdown()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "user-1",
        Username = "testuser",
        ArtistSubmissionPoints = 6,
        ArtistApprovalPoints = 19,
        LyricSubmissionPoints = 25,
        LyricApprovalPoints = 45,
        ReportSubmissionPoints = 3,
        ReportAcknowledgementPoints = 8,
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetOwnProfileDataAsync("user-1");

      // assert
      result.ArtistSubmissionPoints.Should().Be(6);
      result.ArtistApprovalPoints.Should().Be(19);
      result.LyricSubmissionPoints.Should().Be(25);
      result.LyricApprovalPoints.Should().Be(45);
      result.ReportSubmissionPoints.Should().Be(3);
      result.ReportAcknowledgementPoints.Should().Be(8);
      result.TotalPoints.Should().Be(106);
      result.ContributorLabel.Should().Be("Contributor");
    }

    [Test]
    public async Task should_update_last_seen_points()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "user-1",
        Username = "testuser",
        ArtistSubmissionPoints = 100,
        LastSeenPoints = 50,
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      await _service.GetOwnProfileDataAsync("user-1");

      // assert
      var user = await Context.Users.FirstAsync(u => u.CognitoUserId == "user-1");
      user.LastSeenPoints.Should().Be(100);
    }

    [Test]
    public async Task should_return_recent_activity_limited_to_20()
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

      var now = DateTime.UtcNow;
      for (int i = 0; i < 25; i++)
      {
        Context.PointEvents.Add(new PointEvent
        {
          UserId = user.Id,
          ActionType = PointActionType.LyricSubmitted,
          Points = 5,
          EntityId = i + 1,
          EntityName = $"Song {i + 1}",
          CreatedAt = now.AddMinutes(i),
        });
      }

      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetOwnProfileDataAsync("user-1");

      // assert
      result.RecentActivity.Should().HaveCount(20);
      result.RecentActivity.First().EntityName.Should().Be("Song 25");
    }

    [Test]
    public async Task should_return_empty_profile_for_unknown_user()
    {
      // act
      var result = await _service.GetOwnProfileDataAsync("nonexistent");

      // assert
      result.TotalPoints.Should().Be(0);
      result.ContributorLabel.Should().Be("New Contributor");
      result.RecentActivity.Should().BeEmpty();
    }
  }

  [TestFixture]
  public class BbPointsServiceGetPublicProfileDataTests : DatabaseTestBase
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
    public async Task should_return_public_profile_for_existing_user()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "user-1",
        Username = "contributor1",
        Slug = "contributor1",
        ArtistSubmissionPoints = 50,
        LyricSubmissionPoints = 25,
        CreatedAt = DateTime.UtcNow,
      });

      Context.Artists.Add(new Artist { UserId = "user-1", IsDeleted = false, FirstName = "A", LastName = "B", FullName = "A B", Sex = 'M', CreatedAt = DateTime.UtcNow });
      Context.Artists.Add(new Artist { UserId = "user-1", IsDeleted = false, FirstName = "C", LastName = "D", FullName = "C D", Sex = 'M', CreatedAt = DateTime.UtcNow });
      Context.Artists.Add(new Artist { UserId = "user-1", IsDeleted = true, FirstName = "E", LastName = "F", FullName = "E F", Sex = 'M', CreatedAt = DateTime.UtcNow });

      Context.Lyrics.Add(new Lyric { UserId = "user-1", IsDeleted = false, Title = "S1", Body = "B", ArtistId = 0, CreatedAt = DateTime.UtcNow });
      Context.Lyrics.Add(new Lyric { UserId = "user-1", IsDeleted = false, Title = "S2", Body = "B", ArtistId = 0, CreatedAt = DateTime.UtcNow });
      Context.Lyrics.Add(new Lyric { UserId = "user-1", IsDeleted = false, Title = "S3", Body = "B", ArtistId = 0, CreatedAt = DateTime.UtcNow });

      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetPublicProfileDataAsync("contributor1");

      // assert
      result.Should().NotBeNull();
      result.Username.Should().Be("contributor1");
      result.TotalPoints.Should().Be(75);
      result.ContributorLabel.Should().Be("Contributor");
      result.ArtistsSubmittedCount.Should().Be(2); // excludes deleted
      result.LyricsSubmittedCount.Should().Be(3);
    }

    [Test]
    public async Task should_return_null_for_unknown_slug()
    {
      // act
      var result = await _service.GetPublicProfileDataAsync("nonexistent");

      // assert
      result.Should().BeNull();
    }

    [Test]
    public async Task should_include_pending_submissions_in_counts()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "user-1",
        Username = "testuser",
        Slug = "testuser",
        CreatedAt = DateTime.UtcNow,
      });

      Context.Artists.Add(new Artist { UserId = "user-1", IsDeleted = false, IsApproved = true, FirstName = "A", LastName = "B", FullName = "A B", Sex = 'M', CreatedAt = DateTime.UtcNow });
      Context.Artists.Add(new Artist { UserId = "user-1", IsDeleted = false, IsApproved = false, FirstName = "C", LastName = "D", FullName = "C D", Sex = 'M', CreatedAt = DateTime.UtcNow });

      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetPublicProfileDataAsync("testuser");

      // assert
      result.ArtistsSubmittedCount.Should().Be(2); // includes pending
    }

    [Test]
    public async Task should_query_by_slug_and_return_cognito_user_id()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "abc-123",
        Username = "ali fm",
        Slug = "ali-fm",
        ArtistSubmissionPoints = 10,
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act — query by slug, not username
      var result = await _service.GetPublicProfileDataAsync("ali-fm");

      // assert
      result.Should().NotBeNull();
      result.Username.Should().Be("ali fm");
      result.CognitoUserId.Should().Be("abc-123");
    }

    [Test]
    public async Task should_return_null_when_no_user_matches_slug()
    {
      // act
      var result = await _service.GetPublicProfileDataAsync("no-such-slug");

      // assert
      result.Should().BeNull();
    }
  }

  [TestFixture]
  public class BbPointsServiceGetSubmitterPointsTests : DatabaseTestBase
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
    public async Task should_return_points_for_user_with_record()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "user-1",
        Username = "songwriter",
        Slug = "songwriter",
        ArtistSubmissionPoints = 10,
        LyricApprovalPoints = 200,
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetSubmitterPointsAsync("user-1");

      // assert
      result.TotalPoints.Should().Be(210);
      result.ContributorLabel.Should().Be("Regular Contributor");
      result.Username.Should().Be("songwriter");
    }

    [Test]
    public async Task should_return_slug_from_user_record()
    {
      // arrange
      Context.Users.Add(new User
      {
        CognitoUserId = "user-1",
        Username = "ali fm",
        Slug = "ali-fm",
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetSubmitterPointsAsync("user-1");

      // assert
      result.Slug.Should().Be("ali-fm");
    }

    [Test]
    public async Task should_return_null_slug_in_fallback_path()
    {
      // arrange
      _cognitoServiceMock
        .Setup(x => x.GetPreferredUsernameAsync("old-user-789"))
        .ReturnsAsync("resolveduser");

      // act
      var result = await _service.GetSubmitterPointsAsync("old-user-789");

      // assert
      result.Slug.Should().BeNull();
    }

    [Test]
    public async Task should_return_fallback_for_missing_user_record()
    {
      // arrange
      _cognitoServiceMock
        .Setup(x => x.GetPreferredUsernameAsync("old-user-789"))
        .ReturnsAsync("resolveduser");

      // act
      var result = await _service.GetSubmitterPointsAsync("old-user-789");

      // assert
      result.TotalPoints.Should().Be(0);
      result.ContributorLabel.Should().Be("New Contributor");
      result.Username.Should().Be("resolveduser");
    }

    [Test]
    public async Task should_use_truncated_id_when_cognito_fails()
    {
      // arrange
      _cognitoServiceMock
        .Setup(x => x.GetPreferredUsernameAsync("abcdefgh-1234"))
        .ThrowsAsync(new Exception("cognito error"));

      // act
      var result = await _service.GetSubmitterPointsAsync("abcdefgh-1234");

      // assert
      result.TotalPoints.Should().Be(0);
      result.Username.Should().Be("user-abcdefgh");
    }
  }
}
