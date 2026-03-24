namespace Bejebeje.Mvc.Tests.Models
{
  using Bejebeje.Models.BbPoints;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class PublicProfileViewModelTests
  {
    [Test]
    public void should_have_username_property()
    {
      var vm = new PublicProfileViewModel { Username = "contributor1" };
      vm.Username.Should().Be("contributor1");
    }

    [Test]
    public void should_have_total_points_property()
    {
      var vm = new PublicProfileViewModel { TotalPoints = 250 };
      vm.TotalPoints.Should().Be(250);
    }

    [Test]
    public void should_have_contributor_label_property()
    {
      var vm = new PublicProfileViewModel { ContributorLabel = "Regular Contributor" };
      vm.ContributorLabel.Should().Be("Regular Contributor");
    }

    [Test]
    public void should_have_artists_submitted_count_property()
    {
      var vm = new PublicProfileViewModel { ArtistsSubmittedCount = 8 };
      vm.ArtistsSubmittedCount.Should().Be(8);
    }

    [Test]
    public void should_have_lyrics_submitted_count_property()
    {
      var vm = new PublicProfileViewModel { LyricsSubmittedCount = 15 };
      vm.LyricsSubmittedCount.Should().Be(15);
    }

    [Test]
    public void should_have_cognito_user_id_property()
    {
      var vm = new PublicProfileViewModel { CognitoUserId = "abc-123" };
      vm.CognitoUserId.Should().Be("abc-123");
    }
  }
}
