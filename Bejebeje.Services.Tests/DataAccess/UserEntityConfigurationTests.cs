namespace Bejebeje.Services.Tests.DataAccess
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.DataAccess.Context;
  using Bejebeje.Shared.Domain;
  using Bejebeje.Services.Tests.Helpers;
  using FluentAssertions;
  using Microsoft.EntityFrameworkCore;
  using NUnit.Framework;

  [TestFixture]
  public class UserEntityConfigurationTests : DatabaseTestBase
  {
    [SetUp]
    public void Setup()
    {
      SetupDataContext();
    }

    [Test]
    public async Task should_persist_user_with_all_fields()
    {
      // arrange
      var user = new User
      {
        CognitoUserId = "abc-123",
        Username = "testuser",
        CreatedAt = DateTime.UtcNow,
      };

      // act
      Context.Users.Add(user);
      await Context.SaveChangesAsync();

      // assert
      var saved = await Context.Users.FirstAsync();
      saved.CognitoUserId.Should().Be("abc-123");
      saved.Username.Should().Be("testuser");
      saved.ArtistSubmissionPoints.Should().Be(0);
      saved.ArtistApprovalPoints.Should().Be(0);
      saved.LyricSubmissionPoints.Should().Be(0);
      saved.LyricApprovalPoints.Should().Be(0);
      saved.ReportSubmissionPoints.Should().Be(0);
      saved.ReportAcknowledgementPoints.Should().Be(0);
      saved.LastSeenPoints.Should().Be(0);
    }

    [Test]
    public async Task should_auto_generate_id()
    {
      // arrange
      var user = new User
      {
        CognitoUserId = "abc-123",
        Username = "testuser",
        CreatedAt = DateTime.UtcNow,
      };

      // act
      Context.Users.Add(user);
      await Context.SaveChangesAsync();

      // assert
      var saved = await Context.Users.FirstAsync();
      saved.Id.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task should_be_queryable_as_dbset()
    {
      // arrange
      Context.Users.Add(new User { CognitoUserId = "a", Username = "user1", CreatedAt = DateTime.UtcNow });
      Context.Users.Add(new User { CognitoUserId = "b", Username = "user2", CreatedAt = DateTime.UtcNow });
      await Context.SaveChangesAsync();

      // act
      var count = await Context.Users.CountAsync();

      // assert
      count.Should().Be(2);
    }
  }
}
