namespace Bejebeje.Mvc.Tests.Models
{
  using Bejebeje.Models.Homepage;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class OpportunityCardViewModelTests
  {
    [Test]
    public void should_have_artist_id_property()
    {
      var vm = new OpportunityCardViewModel { ArtistId = 5 };
      vm.ArtistId.Should().Be(5);
    }

    [Test]
    public void should_have_artist_name_property()
    {
      var vm = new OpportunityCardViewModel { ArtistName = "Zakaria" };
      vm.ArtistName.Should().Be("Zakaria");
    }

    [Test]
    public void should_have_artist_slug_property()
    {
      var vm = new OpportunityCardViewModel { ArtistSlug = "zakaria" };
      vm.ArtistSlug.Should().Be("zakaria");
    }

    [Test]
    public void should_have_has_image_property()
    {
      var vm = new OpportunityCardViewModel { HasImage = true };
      vm.HasImage.Should().BeTrue();
    }

    [Test]
    public void should_have_approved_lyric_count_property()
    {
      var vm = new OpportunityCardViewModel { ApprovedLyricCount = 2 };
      vm.ApprovedLyricCount.Should().Be(2);
    }

    [Test]
    public void should_have_opportunity_type_property()
    {
      var vm = new OpportunityCardViewModel { OpportunityType = "artists_with_few_lyrics" };
      vm.OpportunityType.Should().Be("artists_with_few_lyrics");
    }

    [Test]
    public void should_default_has_image_to_false()
    {
      var vm = new OpportunityCardViewModel();
      vm.HasImage.Should().BeFalse();
    }

    [Test]
    public void should_default_approved_lyric_count_to_zero()
    {
      var vm = new OpportunityCardViewModel();
      vm.ApprovedLyricCount.Should().Be(0);
    }
  }
}
