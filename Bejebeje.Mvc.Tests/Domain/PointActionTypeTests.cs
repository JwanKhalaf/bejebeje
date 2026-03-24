namespace Bejebeje.Mvc.Tests.Domain
{
  using Bejebeje.Domain;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class PointActionTypeTests
  {
    [Test]
    public void should_have_artist_submitted_as_one()
    {
      ((int)PointActionType.ArtistSubmitted).Should().Be(1);
    }

    [Test]
    public void should_have_artist_approved_as_two()
    {
      ((int)PointActionType.ArtistApproved).Should().Be(2);
    }

    [Test]
    public void should_have_lyric_submitted_as_three()
    {
      ((int)PointActionType.LyricSubmitted).Should().Be(3);
    }

    [Test]
    public void should_have_lyric_approved_as_four()
    {
      ((int)PointActionType.LyricApproved).Should().Be(4);
    }

    [Test]
    public void should_have_report_submitted_as_five()
    {
      ((int)PointActionType.ReportSubmitted).Should().Be(5);
    }

    [Test]
    public void should_have_report_acknowledged_as_six()
    {
      ((int)PointActionType.ReportAcknowledged).Should().Be(6);
    }

    [Test]
    public void should_have_exactly_six_values()
    {
      var values = System.Enum.GetValues<PointActionType>();
      values.Should().HaveCount(6);
    }
  }
}
