namespace Bejebeje.Mvc.Tests.Domain
{
  using System;
  using Bejebeje.Shared.Domain;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class PointEventTests
  {
    [Test]
    public void should_have_id_property()
    {
      var pointEvent = new PointEvent { Id = 1 };
      pointEvent.Id.Should().Be(1);
    }

    [Test]
    public void should_have_user_id_property()
    {
      var pointEvent = new PointEvent { UserId = 5 };
      pointEvent.UserId.Should().Be(5);
    }

    [Test]
    public void should_have_action_type_property()
    {
      var pointEvent = new PointEvent { ActionType = PointActionType.LyricSubmitted };
      pointEvent.ActionType.Should().Be(PointActionType.LyricSubmitted);
    }

    [Test]
    public void should_have_points_property()
    {
      var pointEvent = new PointEvent { Points = 5 };
      pointEvent.Points.Should().Be(5);
    }

    [Test]
    public void should_have_entity_id_property()
    {
      var pointEvent = new PointEvent { EntityId = 42 };
      pointEvent.EntityId.Should().Be(42);
    }

    [Test]
    public void should_have_entity_name_property()
    {
      var pointEvent = new PointEvent { EntityName = "Song Title" };
      pointEvent.EntityName.Should().Be("Song Title");
    }

    [Test]
    public void should_have_created_at_property()
    {
      var now = DateTime.UtcNow;
      var pointEvent = new PointEvent { CreatedAt = now };
      pointEvent.CreatedAt.Should().Be(now);
    }

    [Test]
    public void should_have_user_navigation_property()
    {
      var user = new User { Id = 1, CognitoUserId = "abc", Username = "test" };
      var pointEvent = new PointEvent { User = user };
      pointEvent.User.Should().BeSameAs(user);
    }
  }
}
