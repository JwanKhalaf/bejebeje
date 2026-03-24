namespace Bejebeje.Mvc.Tests.Models
{
  using Bejebeje.Models.BbPoints;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class SubmitterPointsViewModelTests
  {
    [Test]
    public void should_have_total_points_property()
    {
      var vm = new SubmitterPointsViewModel { TotalPoints = 210 };
      vm.TotalPoints.Should().Be(210);
    }

    [Test]
    public void should_have_contributor_label_property()
    {
      var vm = new SubmitterPointsViewModel { ContributorLabel = "Regular Contributor" };
      vm.ContributorLabel.Should().Be("Regular Contributor");
    }

    [Test]
    public void should_have_username_property()
    {
      var vm = new SubmitterPointsViewModel { Username = "songwriter" };
      vm.Username.Should().Be("songwriter");
    }

    [Test]
    public void should_have_slug_property()
    {
      var vm = new SubmitterPointsViewModel { Slug = "songwriter" };
      vm.Slug.Should().Be("songwriter");
    }

    [Test]
    public void should_have_slug_default_to_null()
    {
      var vm = new SubmitterPointsViewModel();
      vm.Slug.Should().BeNull();
    }
  }
}
