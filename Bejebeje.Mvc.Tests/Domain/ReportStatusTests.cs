namespace Bejebeje.Mvc.Tests.Domain
{
  using Bejebeje.Domain;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class ReportStatusTests
  {
    [Test]
    public void should_have_pending_as_zero()
    {
      ((int)ReportStatus.Pending).Should().Be(0);
    }

    [Test]
    public void should_have_acknowledged_as_one()
    {
      ((int)ReportStatus.Acknowledged).Should().Be(1);
    }

    [Test]
    public void should_have_dismissed_as_two()
    {
      ((int)ReportStatus.Dismissed).Should().Be(2);
    }

    [Test]
    public void should_have_exactly_three_values()
    {
      var values = System.Enum.GetValues<ReportStatus>();
      values.Should().HaveCount(3);
    }
  }
}
