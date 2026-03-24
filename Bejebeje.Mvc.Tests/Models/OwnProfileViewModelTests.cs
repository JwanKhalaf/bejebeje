namespace Bejebeje.Mvc.Tests.Models
{
  using System.Collections.Generic;
  using Bejebeje.Models.BbPoints;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class OwnProfileViewModelTests
  {
    [Test]
    public void should_have_all_six_category_point_properties()
    {
      var vm = new OwnProfileViewModel
      {
        ArtistSubmissionPoints = 1,
        ArtistApprovalPoints = 2,
        LyricSubmissionPoints = 3,
        LyricApprovalPoints = 4,
        ReportSubmissionPoints = 5,
        ReportAcknowledgementPoints = 6,
      };

      vm.ArtistSubmissionPoints.Should().Be(1);
      vm.ArtistApprovalPoints.Should().Be(2);
      vm.LyricSubmissionPoints.Should().Be(3);
      vm.LyricApprovalPoints.Should().Be(4);
      vm.ReportSubmissionPoints.Should().Be(5);
      vm.ReportAcknowledgementPoints.Should().Be(6);
    }

    [Test]
    public void should_have_like_points_property()
    {
      var vm = new OwnProfileViewModel { LikePoints = 3 };
      vm.LikePoints.Should().Be(3);
    }

    [Test]
    public void should_have_total_points_property()
    {
      var vm = new OwnProfileViewModel { TotalPoints = 100 };
      vm.TotalPoints.Should().Be(100);
    }

    [Test]
    public void should_have_contributor_label_property()
    {
      var vm = new OwnProfileViewModel { ContributorLabel = "Top Contributor" };
      vm.ContributorLabel.Should().Be("Top Contributor");
    }

    [Test]
    public void should_have_username_property()
    {
      var vm = new OwnProfileViewModel { Username = "testuser" };
      vm.Username.Should().Be("testuser");
    }

    [Test]
    public void should_have_recent_activity_property()
    {
      var activities = new List<PointActivityViewModel>
      {
        new PointActivityViewModel { ActionDescription = "Submitted artist", Points = 5 },
      };

      var vm = new OwnProfileViewModel { RecentActivity = activities };
      vm.RecentActivity.Should().HaveCount(1);
    }

    [Test]
    public void should_default_recent_activity_to_empty_list()
    {
      var vm = new OwnProfileViewModel();
      vm.RecentActivity.Should().NotBeNull();
      vm.RecentActivity.Should().BeEmpty();
    }
  }
}
