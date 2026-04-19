namespace Bejebeje.Mvc.Tests.Domain
{
  using Bejebeje.Shared.Domain;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class BbPointsConstantsTests
  {
    [Test]
    public void should_have_artist_submitted_no_photo_as_one()
    {
      BbPointsConstants.ArtistSubmittedNoPhoto.Should().Be(1);
    }

    [Test]
    public void should_have_artist_submitted_with_photo_as_five()
    {
      BbPointsConstants.ArtistSubmittedWithPhoto.Should().Be(5);
    }

    [Test]
    public void should_have_artist_approved_no_photo_as_nine()
    {
      BbPointsConstants.ArtistApprovedNoPhoto.Should().Be(9);
    }

    [Test]
    public void should_have_artist_approved_with_photo_as_ten()
    {
      BbPointsConstants.ArtistApprovedWithPhoto.Should().Be(10);
    }

    [Test]
    public void should_have_lyric_submitted_as_five()
    {
      BbPointsConstants.LyricSubmitted.Should().Be(5);
    }

    [Test]
    public void should_have_lyric_approved_as_fifteen()
    {
      BbPointsConstants.LyricApproved.Should().Be(15);
    }

    [Test]
    public void should_have_report_submitted_as_one()
    {
      BbPointsConstants.ReportSubmitted.Should().Be(1);
    }

    [Test]
    public void should_have_report_acknowledged_as_four()
    {
      BbPointsConstants.ReportAcknowledged.Should().Be(4);
    }

    [Test]
    public void should_have_likes_per_point_as_ten()
    {
      BbPointsConstants.LikesPerPoint.Should().Be(10);
    }
  }
}
