namespace Bejebeje.Services.Tests.Services
{
  using Bejebeje.Services.Services;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class HomepageServiceConstantsTests
  {
    [Test]
    public void should_have_lyric_threshold_of_three()
    {
      HomepageServiceConstants.LyricThreshold.Should().Be(3);
    }

    [Test]
    public void should_have_recency_window_of_ninety_days()
    {
      HomepageServiceConstants.RecencyWindowDays.Should().Be(90);
    }

    [Test]
    public void should_have_max_opportunity_cards_of_eight()
    {
      HomepageServiceConstants.MaxOpportunityCards.Should().Be(8);
    }

    [Test]
    public void should_have_stats_cache_ttl_of_thirty_minutes()
    {
      HomepageServiceConstants.StatsCacheTtlMinutes.Should().Be(30);
    }

    [Test]
    public void should_have_stats_cache_key()
    {
      HomepageServiceConstants.StatsCacheKey.Should().Be("homepage:community_impact_stats");
    }

    [Test]
    public void should_have_max_new_artist_cards_of_four()
    {
      HomepageServiceConstants.MaxNewArtistCards.Should().Be(4);
    }
  }
}
