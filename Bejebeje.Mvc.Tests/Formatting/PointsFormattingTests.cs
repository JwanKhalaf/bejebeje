namespace Bejebeje.Mvc.Tests.Formatting
{
  using FluentAssertions;
  using Humanizer;
  using NUnit.Framework;

  /// <summary>
  /// tests for the points formatting logic used across razor views.
  /// the views use ((double)points).ToMetric(decimals: 1) from Humanizer.
  /// </summary>
  [TestFixture]
  public class PointsFormattingTests
  {
    [Test]
    public void should_display_small_numbers_as_is()
    {
      // arrange & act
      string result = ((double)42).ToMetric(decimals: 1);

      // assert
      result.Should().Be("42");
    }

    [Test]
    public void should_display_zero_as_zero()
    {
      // arrange & act
      string result = ((double)0).ToMetric(decimals: 1);

      // assert
      result.Should().Be("0");
    }

    [Test]
    public void should_display_999_without_suffix()
    {
      // arrange & act
      string result = ((double)999).ToMetric(decimals: 1);

      // assert
      result.Should().Be("999");
    }

    [Test]
    public void should_display_1000_as_1k()
    {
      // arrange & act
      string result = ((double)1000).ToMetric(decimals: 1);

      // assert
      result.Should().Be("1k");
    }

    [Test]
    public void should_display_1500_as_1_5k()
    {
      // arrange & act
      string result = ((double)1500).ToMetric(decimals: 1);

      // assert
      result.Should().Be("1.5k");
    }

    [Test]
    public void should_display_14000_as_14k()
    {
      // arrange & act
      string result = ((double)14000).ToMetric(decimals: 1);

      // assert
      result.Should().Be("14k");
    }

    [Test]
    public void should_display_146306_as_146_3k()
    {
      // arrange & act
      string result = ((double)146306).ToMetric(decimals: 1);

      // assert
      result.Should().Be("146.3k");
    }

    [Test]
    public void should_display_500_without_suffix()
    {
      // arrange & act
      string result = ((double)500).ToMetric(decimals: 1);

      // assert
      result.Should().Be("500");
    }

    [Test]
    public void should_display_50_without_suffix()
    {
      // arrange & act
      string result = ((double)50).ToMetric(decimals: 1);

      // assert
      result.Should().Be("50");
    }

    [Test]
    public void should_display_1000000_as_1m()
    {
      // arrange & act
      string result = ((double)1000000).ToMetric(decimals: 1);

      // assert
      result.Should().Be("1M");
    }
  }
}
