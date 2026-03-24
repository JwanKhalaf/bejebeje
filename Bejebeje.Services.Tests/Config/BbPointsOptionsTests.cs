namespace Bejebeje.Services.Tests.Config
{
  using Bejebeje.Services.Config;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class BbPointsOptionsTests
  {
    [Test]
    public void should_have_daily_limits_property()
    {
      var options = new BbPointsOptions();
      options.DailyLimits.Should().NotBeNull();
    }

    [Test]
    public void should_have_default_artist_submissions_limit_of_five()
    {
      var options = new BbPointsOptions();
      options.DailyLimits.ArtistSubmissions.Should().Be(5);
    }

    [Test]
    public void should_have_default_lyric_submissions_limit_of_ten()
    {
      var options = new BbPointsOptions();
      options.DailyLimits.LyricSubmissions.Should().Be(10);
    }

    [Test]
    public void should_have_default_report_submissions_limit_of_five()
    {
      var options = new BbPointsOptions();
      options.DailyLimits.ReportSubmissions.Should().Be(5);
    }

    [Test]
    public void should_allow_setting_artist_submissions_limit()
    {
      var options = new BbPointsOptions();
      options.DailyLimits.ArtistSubmissions = 3;
      options.DailyLimits.ArtistSubmissions.Should().Be(3);
    }

    [Test]
    public void should_allow_setting_lyric_submissions_limit()
    {
      var options = new BbPointsOptions();
      options.DailyLimits.LyricSubmissions = 20;
      options.DailyLimits.LyricSubmissions.Should().Be(20);
    }

    [Test]
    public void should_allow_setting_report_submissions_limit()
    {
      var options = new BbPointsOptions();
      options.DailyLimits.ReportSubmissions = 10;
      options.DailyLimits.ReportSubmissions.Should().Be(10);
    }
  }
}
