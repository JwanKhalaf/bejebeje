namespace Bejebeje.Mvc.Tests.Domain
{
  using Bejebeje.Domain;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class ContributorLabelTests
  {
    [Test]
    public void should_return_new_contributor_for_zero_points()
    {
      BbPointsConstants.GetContributorLabel(0).Should().Be("New Contributor");
    }

    [Test]
    public void should_return_new_contributor_for_49_points()
    {
      BbPointsConstants.GetContributorLabel(49).Should().Be("New Contributor");
    }

    [Test]
    public void should_return_contributor_for_50_points()
    {
      BbPointsConstants.GetContributorLabel(50).Should().Be("Contributor");
    }

    [Test]
    public void should_return_contributor_for_199_points()
    {
      BbPointsConstants.GetContributorLabel(199).Should().Be("Contributor");
    }

    [Test]
    public void should_return_regular_contributor_for_200_points()
    {
      BbPointsConstants.GetContributorLabel(200).Should().Be("Regular Contributor");
    }

    [Test]
    public void should_return_regular_contributor_for_499_points()
    {
      BbPointsConstants.GetContributorLabel(499).Should().Be("Regular Contributor");
    }

    [Test]
    public void should_return_top_contributor_for_500_points()
    {
      BbPointsConstants.GetContributorLabel(500).Should().Be("Top Contributor");
    }

    [Test]
    public void should_return_top_contributor_for_1000_points()
    {
      BbPointsConstants.GetContributorLabel(1000).Should().Be("Top Contributor");
    }
  }
}
