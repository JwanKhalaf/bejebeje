namespace Bejebeje.Mvc.Tests.Domain
{
  using System;
  using Bejebeje.Domain;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class UserTests
  {
    [Test]
    public void should_have_id_property()
    {
      var user = new User { Id = 42 };
      user.Id.Should().Be(42);
    }

    [Test]
    public void should_have_cognito_user_id_property()
    {
      var user = new User { CognitoUserId = "abc-123" };
      user.CognitoUserId.Should().Be("abc-123");
    }

    [Test]
    public void should_have_username_property()
    {
      var user = new User { Username = "testuser" };
      user.Username.Should().Be("testuser");
    }

    [Test]
    public void should_have_artist_submission_points_default_to_zero()
    {
      var user = new User();
      user.ArtistSubmissionPoints.Should().Be(0);
    }

    [Test]
    public void should_have_artist_approval_points_default_to_zero()
    {
      var user = new User();
      user.ArtistApprovalPoints.Should().Be(0);
    }

    [Test]
    public void should_have_lyric_submission_points_default_to_zero()
    {
      var user = new User();
      user.LyricSubmissionPoints.Should().Be(0);
    }

    [Test]
    public void should_have_lyric_approval_points_default_to_zero()
    {
      var user = new User();
      user.LyricApprovalPoints.Should().Be(0);
    }

    [Test]
    public void should_have_report_submission_points_default_to_zero()
    {
      var user = new User();
      user.ReportSubmissionPoints.Should().Be(0);
    }

    [Test]
    public void should_have_report_acknowledgement_points_default_to_zero()
    {
      var user = new User();
      user.ReportAcknowledgementPoints.Should().Be(0);
    }

    [Test]
    public void should_have_last_seen_points_default_to_zero()
    {
      var user = new User();
      user.LastSeenPoints.Should().Be(0);
    }

    [Test]
    public void should_have_created_at_property()
    {
      var now = DateTime.UtcNow;
      var user = new User { CreatedAt = now };
      user.CreatedAt.Should().Be(now);
    }

    [Test]
    public void should_have_modified_at_nullable_property()
    {
      var user = new User();
      user.ModifiedAt.Should().BeNull();
    }

    [Test]
    public void should_allow_setting_modified_at()
    {
      var now = DateTime.UtcNow;
      var user = new User { ModifiedAt = now };
      user.ModifiedAt.Should().Be(now);
    }

    [Test]
    public void should_have_slug_property()
    {
      var user = new User { Slug = "ali-fm" };
      user.Slug.Should().Be("ali-fm");
    }
  }
}
