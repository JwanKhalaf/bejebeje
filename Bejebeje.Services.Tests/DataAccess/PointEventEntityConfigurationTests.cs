namespace Bejebeje.Services.Tests.DataAccess
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.DataAccess.Context;
  using Bejebeje.Domain;
  using Bejebeje.Services.Tests.Helpers;
  using FluentAssertions;
  using Microsoft.EntityFrameworkCore;
  using NUnit.Framework;

  [TestFixture]
  public class PointEventEntityConfigurationTests : DatabaseTestBase
  {
    [SetUp]
    public void Setup()
    {
      SetupDataContext();
    }

    [Test]
    public async Task should_persist_point_event_with_all_fields()
    {
      // arrange
      var user = new User
      {
        CognitoUserId = "abc-123",
        Username = "testuser",
        CreatedAt = DateTime.UtcNow,
      };

      Context.Users.Add(user);
      await Context.SaveChangesAsync();

      var pointEvent = new PointEvent
      {
        UserId = user.Id,
        ActionType = PointActionType.LyricSubmitted,
        Points = 5,
        EntityId = 42,
        EntityName = "Song Title",
        CreatedAt = DateTime.UtcNow,
      };

      // act
      Context.PointEvents.Add(pointEvent);
      await Context.SaveChangesAsync();

      // assert
      var saved = await Context.PointEvents.FirstAsync();
      saved.UserId.Should().Be(user.Id);
      saved.ActionType.Should().Be(PointActionType.LyricSubmitted);
      saved.Points.Should().Be(5);
      saved.EntityId.Should().Be(42);
      saved.EntityName.Should().Be("Song Title");
    }

    [Test]
    public async Task should_have_navigation_to_user()
    {
      // arrange
      var user = new User
      {
        CognitoUserId = "abc-123",
        Username = "testuser",
        CreatedAt = DateTime.UtcNow,
      };

      Context.Users.Add(user);
      await Context.SaveChangesAsync();

      var pointEvent = new PointEvent
      {
        UserId = user.Id,
        ActionType = PointActionType.ArtistSubmitted,
        Points = 1,
        EntityId = 1,
        EntityName = "Artist",
        CreatedAt = DateTime.UtcNow,
      };

      Context.PointEvents.Add(pointEvent);
      await Context.SaveChangesAsync();

      // act
      var saved = await Context.PointEvents.Include(pe => pe.User).FirstAsync();

      // assert
      saved.User.Should().NotBeNull();
      saved.User.CognitoUserId.Should().Be("abc-123");
    }

    [Test]
    public async Task should_query_point_events_by_user_in_reverse_chronological_order()
    {
      // arrange
      var user = new User
      {
        CognitoUserId = "abc-123",
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
      var results = await Context.PointEvents
        .Where(pe => pe.UserId == user.Id)
        .OrderByDescending(pe => pe.CreatedAt)
        .Take(20)
        .ToListAsync();

      // assert
      results.Should().HaveCount(20);
      results.First().EntityName.Should().Be("Song 25");
      results.Last().EntityName.Should().Be("Song 6");
    }
  }
}
