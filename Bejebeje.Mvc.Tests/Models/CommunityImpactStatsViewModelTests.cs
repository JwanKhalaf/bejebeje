namespace Bejebeje.Mvc.Tests.Models
{
  using Bejebeje.Models.Homepage;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class CommunityImpactStatsViewModelTests
  {
    [Test]
    public void should_have_total_approved_lyrics_property()
    {
      var vm = new CommunityImpactStatsViewModel { TotalApprovedLyrics = 150 };
      vm.TotalApprovedLyrics.Should().Be(150);
    }

    [Test]
    public void should_have_total_approved_artists_property()
    {
      var vm = new CommunityImpactStatsViewModel { TotalApprovedArtists = 42 };
      vm.TotalApprovedArtists.Should().Be(42);
    }

    [Test]
    public void should_have_total_contributors_property()
    {
      var vm = new CommunityImpactStatsViewModel { TotalContributors = 7 };
      vm.TotalContributors.Should().Be(7);
    }

    [Test]
    public void should_default_total_approved_lyrics_to_zero()
    {
      var vm = new CommunityImpactStatsViewModel();
      vm.TotalApprovedLyrics.Should().Be(0);
    }

    [Test]
    public void should_default_total_approved_artists_to_zero()
    {
      var vm = new CommunityImpactStatsViewModel();
      vm.TotalApprovedArtists.Should().Be(0);
    }

    [Test]
    public void should_default_total_contributors_to_zero()
    {
      var vm = new CommunityImpactStatsViewModel();
      vm.TotalContributors.Should().Be(0);
    }
  }
}
