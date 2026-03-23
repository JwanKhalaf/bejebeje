namespace Bejebeje.Mvc.Tests.Models
{
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.Linq;
  using Bejebeje.Models.Report;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class LyricReportFormViewModelTests
  {
    [Test]
    public void should_fail_validation_when_category_is_below_range()
    {
      // arrange
      var model = new LyricReportFormViewModel
      {
        LyricId = 1,
        Category = -1,
        ArtistSlug = "test-artist",
        LyricSlug = "test-lyric",
      };

      // act
      var results = ValidateModel(model);

      // assert
      results.Should().Contain(r => r.MemberNames.Any(m => m == "Category"));
    }

    [Test]
    public void should_fail_validation_when_category_is_above_range()
    {
      // arrange
      var model = new LyricReportFormViewModel
      {
        LyricId = 1,
        Category = 5,
        ArtistSlug = "test-artist",
        LyricSlug = "test-lyric",
      };

      // act
      var results = ValidateModel(model);

      // assert
      results.Should().Contain(r => r.MemberNames.Any(m => m == "Category"));
    }

    [Test]
    public void should_pass_validation_when_category_is_in_range()
    {
      // arrange
      var model = new LyricReportFormViewModel
      {
        LyricId = 1,
        Category = 2,
        ArtistSlug = "test-artist",
        LyricSlug = "test-lyric",
      };

      // act
      var results = ValidateModel(model);

      // assert
      results.Should().BeEmpty();
    }

    [Test]
    public void should_fail_validation_when_comment_exceeds_2000_characters()
    {
      // arrange
      var model = new LyricReportFormViewModel
      {
        LyricId = 1,
        Category = 0,
        Comment = new string('a', 2001),
        ArtistSlug = "test-artist",
        LyricSlug = "test-lyric",
      };

      // act
      var results = ValidateModel(model);

      // assert
      results.Should().Contain(r => r.MemberNames.Any(m => m == "Comment"));
    }

    [Test]
    public void should_pass_validation_when_comment_is_exactly_2000_characters()
    {
      // arrange
      var model = new LyricReportFormViewModel
      {
        LyricId = 1,
        Category = 0,
        Comment = new string('a', 2000),
        ArtistSlug = "test-artist",
        LyricSlug = "test-lyric",
      };

      // act
      var results = ValidateModel(model);

      // assert
      results.Should().BeEmpty();
    }

    [Test]
    public void should_pass_validation_when_comment_is_null()
    {
      // arrange
      var model = new LyricReportFormViewModel
      {
        LyricId = 1,
        Category = 0,
        Comment = null,
        ArtistSlug = "test-artist",
        LyricSlug = "test-lyric",
      };

      // act
      var results = ValidateModel(model);

      // assert
      results.Should().BeEmpty();
    }

    [Test]
    public void should_fail_validation_when_artist_slug_is_missing()
    {
      // arrange
      var model = new LyricReportFormViewModel
      {
        LyricId = 1,
        Category = 0,
        ArtistSlug = null,
        LyricSlug = "test-lyric",
      };

      // act
      var results = ValidateModel(model);

      // assert
      results.Should().Contain(r => r.MemberNames.Any(m => m == "ArtistSlug"));
    }

    [Test]
    public void should_fail_validation_when_lyric_slug_is_missing()
    {
      // arrange
      var model = new LyricReportFormViewModel
      {
        LyricId = 1,
        Category = 0,
        ArtistSlug = "test-artist",
        LyricSlug = null,
      };

      // act
      var results = ValidateModel(model);

      // assert
      results.Should().Contain(r => r.MemberNames.Any(m => m == "LyricSlug"));
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
      var results = new List<ValidationResult>();
      var context = new ValidationContext(model);
      Validator.TryValidateObject(model, context, results, true);
      return results;
    }
  }
}
