namespace Bejebeje.Mvc.Tests.Models
{
  using System.Collections.Generic;
  using System.Linq;
  using Bejebeje.Models.Artist;
  using Bejebeje.Models.Homepage;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class HomepageViewModelTests
  {
    [Test]
    public void should_have_is_authenticated_property()
    {
      var vm = new HomepageViewModel { IsAuthenticated = true };
      vm.IsAuthenticated.Should().BeTrue();
    }

    [Test]
    public void should_default_is_authenticated_to_false()
    {
      var vm = new HomepageViewModel();
      vm.IsAuthenticated.Should().BeFalse();
    }

    [Test]
    public void should_have_opportunity_cards_property()
    {
      var cards = new List<OpportunityCardViewModel>
      {
        new OpportunityCardViewModel { ArtistId = 1, ArtistName = "Zakaria" },
      };

      var vm = new HomepageViewModel { OpportunityCards = cards.AsReadOnly() };
      vm.OpportunityCards.Should().HaveCount(1);
    }

    [Test]
    public void should_have_community_impact_property()
    {
      var stats = new CommunityImpactStatsViewModel
      {
        TotalApprovedLyrics = 100,
        TotalApprovedArtists = 25,
        TotalContributors = 5,
      };

      var vm = new HomepageViewModel { CommunityImpact = stats };
      vm.CommunityImpact.Should().NotBeNull();
      vm.CommunityImpact.TotalApprovedLyrics.Should().Be(100);
    }

    [Test]
    public void should_default_community_impact_to_null()
    {
      var vm = new HomepageViewModel();
      vm.CommunityImpact.Should().BeNull();
    }

    [Test]
    public void should_have_female_artists_property()
    {
      var artists = new List<RandomFemaleArtistItemViewModel>
      {
        new RandomFemaleArtistItemViewModel { Name = "Gulistan" },
      };

      var vm = new HomepageViewModel { FemaleArtists = artists };
      vm.FemaleArtists.Should().HaveCount(1);
    }

    [Test]
    public void should_allow_empty_opportunity_cards()
    {
      var vm = new HomepageViewModel
      {
        OpportunityCards = new List<OpportunityCardViewModel>().AsReadOnly(),
      };

      vm.OpportunityCards.Should().BeEmpty();
    }

    [Test]
    public void should_allow_empty_female_artists()
    {
      var vm = new HomepageViewModel
      {
        FemaleArtists = Enumerable.Empty<RandomFemaleArtistItemViewModel>(),
      };

      vm.FemaleArtists.Should().BeEmpty();
    }
  }
}
