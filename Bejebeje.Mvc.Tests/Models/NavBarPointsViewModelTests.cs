namespace Bejebeje.Mvc.Tests.Models
{
  using Bejebeje.Models.BbPoints;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class NavBarPointsViewModelTests
  {
    [Test]
    public void should_have_total_points_property()
    {
      var vm = new NavBarPointsViewModel { TotalPoints = 42 };
      vm.TotalPoints.Should().Be(42);
    }

    [Test]
    public void should_have_contributor_label_property()
    {
      var vm = new NavBarPointsViewModel { ContributorLabel = "Contributor" };
      vm.ContributorLabel.Should().Be("Contributor");
    }

    [Test]
    public void should_have_has_points_changed_property()
    {
      var vm = new NavBarPointsViewModel { HasPointsChanged = true };
      vm.HasPointsChanged.Should().BeTrue();
    }

    [Test]
    public void should_default_has_points_changed_to_false()
    {
      var vm = new NavBarPointsViewModel();
      vm.HasPointsChanged.Should().BeFalse();
    }
  }
}
