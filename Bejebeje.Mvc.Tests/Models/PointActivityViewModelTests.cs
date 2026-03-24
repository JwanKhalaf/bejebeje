namespace Bejebeje.Mvc.Tests.Models
{
  using System;
  using Bejebeje.Models.BbPoints;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class PointActivityViewModelTests
  {
    [Test]
    public void should_have_action_description_property()
    {
      var vm = new PointActivityViewModel { ActionDescription = "Submitted artist" };
      vm.ActionDescription.Should().Be("Submitted artist");
    }

    [Test]
    public void should_have_entity_name_property()
    {
      var vm = new PointActivityViewModel { EntityName = "Song Title" };
      vm.EntityName.Should().Be("Song Title");
    }

    [Test]
    public void should_have_points_property()
    {
      var vm = new PointActivityViewModel { Points = 5 };
      vm.Points.Should().Be(5);
    }

    [Test]
    public void should_have_date_property()
    {
      var date = new DateTime(2026, 3, 24, 12, 0, 0, DateTimeKind.Utc);
      var vm = new PointActivityViewModel { Date = date };
      vm.Date.Should().Be(date);
    }
  }
}
