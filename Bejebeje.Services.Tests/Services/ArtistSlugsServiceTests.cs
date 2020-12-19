namespace Bejebeje.Services.Tests.Services
{
  using System;
  using Domain;
  using Bejebeje.Services.Services;
  using FluentAssertions;
  using NodaTime;
  using NUnit.Framework;

  [TestFixture]
  public class ArtistSlugsServiceTests
  {
    private ArtistSlugsService artistSlugsService;

    [SetUp]
    public void Setup()
    {
      artistSlugsService = new ArtistSlugsService();
    }

    [Test]
    public void GetArtistSlug_WithNullParameter_ThrowsArgumentNullException()
    {
      // arrange
      string fullName = null;

      // act
      Action action = () => artistSlugsService.GetArtistSlug(fullName);

      // assert
      action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GetArtistSlug_WithEmptyStringParameter_ThrowsArgumentNullException()
    {
      // arrange
      string fullName = string.Empty;

      // act
      Action action = () => artistSlugsService.GetArtistSlug(fullName);

      // assert
      action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GetArtistSlug_WithValidParameter_ReturnsCorrectSlug()
    {
      // arrange
      string fullName = "Çopî Fetah";
      string expectedSlug = "copi-fetah";

      // act
      string result = artistSlugsService.GetArtistSlug(fullName);

      // assert
      result.Should().Be(expectedSlug);
    }

    [Test]
    public void BuildArtistSlug_WithNullParameter_ThrowsArgumentNullException()
    {
      // arrange
      string fullName = null;

      // act
      Action action = () => artistSlugsService.BuildArtistSlug(fullName);

      // assert
      action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void BuildArtistSlug_WithEmptyStringParameter_ThrowsArgumentNullException()
    {
      // arrange
      string fullName = string.Empty;

      // act
      Action action = () => artistSlugsService.BuildArtistSlug(fullName);

      // assert
      action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void BuildArtistSlug_WithValidParameter_ReturnsCorrectArtistSlug()
    {
      // arrange
      string fullName = "Çopî Fetah";
      string expectedSlug = "copi-fetah";

      // act
      ArtistSlug result = artistSlugsService.BuildArtistSlug(fullName);

      // assert
      result.Should().NotBeNull();
      result.Should().BeOfType<ArtistSlug>();
      result.Name.Should().Be(expectedSlug);
      result.CreatedAt.Should().BeCloseTo(SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc(), 100);
      result.IsPrimary.Should().BeTrue();
    }
  }
}
