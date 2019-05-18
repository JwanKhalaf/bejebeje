using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bejebeje.Domain;
using Bejebeje.Services.Services;
using Bejebeje.Services.Tests.Helpers;
using Bejebeje.ViewModels.Artist;
using FluentAssertions;
using NUnit.Framework;

namespace Bejebeje.Services.Tests.Services
{
  [TestFixture]
  public class ArtistsServiceTests : DatabaseTestBase
  {
    private ArtistsService artistsService;

    [SetUp]
    public void Setup()
    {
      SetupDataContext();

      artistsService = new ArtistsService(Context);
    }

    [Test]
    public async Task GetArtistsAsync_WithNoData_ReturnsAnEmptyListOfArtists()
    {
      // act
      IList<ArtistCardViewModel> result = await artistsService.GetArtistsAsync();

      // assert
      result.Should().BeOfType<List<ArtistCardViewModel>>();
      result.Should().BeEmpty();
    }

    [Test]
    public async Task GetArtistsAsync_WithData_ReturnsAListOfArtists()
    {
      // arrange
      string artistFirstName = "Johnny";
      string artistLastName = "Cash";
      string artistSlug = "johnny-cash";
      int expectedArtistImageId = 1;

      Artist artistFromDb = new Artist
      {
        FirstName = artistFirstName,
        LastName = artistLastName,
        Image = new ArtistImage
        {
          Data = new byte[10],
          CreatedAt = DateTime.UtcNow
        },
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = artistSlug,
            IsPrimary = true,
            CreatedAt = DateTime.UtcNow
          }
        }
      };

      Context.Artists.Add(artistFromDb);
      Context.SaveChanges();

      // act
      IList<ArtistCardViewModel> result = await artistsService.GetArtistsAsync();

      // assert
      result.Should().BeOfType<List<ArtistCardViewModel>>();
      result.Should().HaveCount(1);
      result.First().FirstName.Should().Be(artistFirstName);
      result.First().LastName.Should().Be(artistLastName);
      result.First().ImageId.Should().Be(expectedArtistImageId);
      result.First().Slug.Should().Be(artistSlug);
    }
  }
}
