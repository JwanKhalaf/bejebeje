namespace Bejebeje.Mvc.Tests.Domain
{
  using Bejebeje.Shared.Domain;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class ReportCategoryTests
  {
    [Test]
    public void should_have_lyrics_not_in_kurdish_as_zero()
    {
      ((int)ReportCategory.LyricsNotInKurdish).Should().Be(0);
    }

    [Test]
    public void should_have_lyrics_contain_mistakes_as_one()
    {
      ((int)ReportCategory.LyricsContainMistakes).Should().Be(1);
    }

    [Test]
    public void should_have_duplicate_as_two()
    {
      ((int)ReportCategory.Duplicate).Should().Be(2);
    }

    [Test]
    public void should_have_wrong_artist_as_three()
    {
      ((int)ReportCategory.WrongArtist).Should().Be(3);
    }

    [Test]
    public void should_have_offensive_or_inappropriate_as_four()
    {
      ((int)ReportCategory.OffensiveOrInappropriate).Should().Be(4);
    }

    [Test]
    public void should_have_exactly_five_values()
    {
      var values = System.Enum.GetValues<ReportCategory>();
      values.Should().HaveCount(5);
    }
  }
}
