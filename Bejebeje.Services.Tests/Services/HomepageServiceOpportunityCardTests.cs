namespace Bejebeje.Services.Tests.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Shared.Domain;
  using Bejebeje.Services.Services;
  using Bejebeje.Services.Services.Interfaces;
  using Bejebeje.Services.Tests.Helpers;
  using FluentAssertions;
  using Microsoft.Extensions.Caching.Memory;
  using Microsoft.Extensions.Logging;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class HomepageServiceOpportunityCardTests : DatabaseTestBase
  {
    private HomepageService _service;
    private Mock<IArtistsService> _artistsServiceMock;
    private IMemoryCache _memoryCache;
    private Mock<ILogger<HomepageService>> _loggerMock;

    [SetUp]
    public void Setup()
    {
      SetupDataContext();

      _artistsServiceMock = new Mock<IArtistsService>();
      _artistsServiceMock
        .Setup(x => x.GetTopTenFemaleArtistsByLyricsCountAsync())
        .ReturnsAsync(Enumerable.Empty<Models.Artist.RandomFemaleArtistItemViewModel>());

      _memoryCache = new MemoryCache(new MemoryCacheOptions());
      _loggerMock = new Mock<ILogger<HomepageService>>();

      _service = new HomepageService(
        Context,
        _artistsServiceMock.Object,
        _memoryCache,
        _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
      _memoryCache?.Dispose();
    }

    // --- q1: artists with few lyrics ---

    [Test]
    public async Task should_return_approved_artists_with_fewer_than_threshold_lyrics()
    {
      // arrange
      var artist = CreateApprovedArtist("Zakaria", 1);
      Context.Artists.Add(artist);
      Context.ArtistSlugs.Add(CreatePrimarySlug(artist.Id, "zakaria"));
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert
      result.OpportunityCards.Should().Contain(c => c.ArtistName == "Zakaria");
    }

    [Test]
    public async Task should_not_return_unapproved_artists()
    {
      // arrange
      var artist = CreateArtist("Unapproved", 1, isApproved: false);
      Context.Artists.Add(artist);
      Context.ArtistSlugs.Add(CreatePrimarySlug(artist.Id, "unapproved"));
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert
      result.OpportunityCards.Should().NotContain(c => c.ArtistName == "Unapproved");
    }

    [Test]
    public async Task should_not_return_deleted_artists()
    {
      // arrange
      var artist = CreateArtist("Deleted", 1, isDeleted: true);
      Context.Artists.Add(artist);
      Context.ArtistSlugs.Add(CreatePrimarySlug(artist.Id, "deleted"));
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert
      result.OpportunityCards.Should().NotContain(c => c.ArtistName == "Deleted");
    }

    [Test]
    public async Task should_not_return_artists_with_threshold_or_more_approved_lyrics()
    {
      // arrange
      var artist = CreateApprovedArtist("Popular", 1);
      Context.Artists.Add(artist);
      Context.ArtistSlugs.Add(CreatePrimarySlug(artist.Id, "popular"));

      // add 3 approved lyrics (meets threshold)
      for (int i = 0; i < 3; i++)
      {
        Context.Lyrics.Add(CreateApprovedLyric(artist.Id, $"Song {i}", 100 + i));
      }

      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert
      result.OpportunityCards.Should().NotContain(c => c.ArtistName == "Popular");
    }

    [Test]
    public async Task should_not_count_deleted_lyrics_toward_threshold()
    {
      // arrange
      var artist = CreateApprovedArtist("Has Deleted Lyrics", 1);
      Context.Artists.Add(artist);
      Context.ArtistSlugs.Add(CreatePrimarySlug(artist.Id, "has-deleted-lyrics"));

      // add 3 lyrics but 2 are deleted — only 1 counts
      Context.Lyrics.Add(CreateApprovedLyric(artist.Id, "Active Song", 100));
      Context.Lyrics.Add(CreateLyric(artist.Id, "Deleted Song 1", 101, isDeleted: true));
      Context.Lyrics.Add(CreateLyric(artist.Id, "Deleted Song 2", 102, isDeleted: true));

      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert
      result.OpportunityCards.Should().Contain(c => c.ArtistName == "Has Deleted Lyrics");
      result.OpportunityCards.First(c => c.ArtistName == "Has Deleted Lyrics")
        .ApprovedLyricCount.Should().Be(1);
    }

    [Test]
    public async Task should_not_count_unapproved_lyrics_toward_threshold()
    {
      // arrange
      var artist = CreateApprovedArtist("Has Unapproved Lyrics", 1);
      Context.Artists.Add(artist);
      Context.ArtistSlugs.Add(CreatePrimarySlug(artist.Id, "has-unapproved-lyrics"));

      // add 3 lyrics but 2 are unapproved — only 1 counts
      Context.Lyrics.Add(CreateApprovedLyric(artist.Id, "Active Song", 100));
      Context.Lyrics.Add(CreateLyric(artist.Id, "Pending Song 1", 101, isApproved: false));
      Context.Lyrics.Add(CreateLyric(artist.Id, "Pending Song 2", 102, isApproved: false));

      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert
      result.OpportunityCards.Should().Contain(c => c.ArtistName == "Has Unapproved Lyrics");
    }

    [Test]
    public async Task should_order_q1_by_lyric_count_ascending_then_created_at_descending()
    {
      // arrange
      // use dates outside the 90-day recency window so these are q1 only
      var artist1 = CreateApprovedArtist("Two Lyrics", 1, createdAt: DateTime.UtcNow.AddDays(-200));
      var artist2 = CreateApprovedArtist("Zero Lyrics", 2, createdAt: DateTime.UtcNow.AddDays(-150));
      var artist3 = CreateApprovedArtist("One Lyric", 3, createdAt: DateTime.UtcNow.AddDays(-180));

      Context.Artists.AddRange(artist1, artist2, artist3);
      Context.ArtistSlugs.Add(CreatePrimarySlug(1, "two-lyrics"));
      Context.ArtistSlugs.Add(CreatePrimarySlug(2, "zero-lyrics"));
      Context.ArtistSlugs.Add(CreatePrimarySlug(3, "one-lyric"));

      Context.Lyrics.Add(CreateApprovedLyric(1, "Song A", 100));
      Context.Lyrics.Add(CreateApprovedLyric(1, "Song B", 101));
      Context.Lyrics.Add(CreateApprovedLyric(3, "Song C", 102));

      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert — order should be: Zero Lyrics (0), One Lyric (1), Two Lyrics (2)
      var cards = result.OpportunityCards
        .Where(c => c.OpportunityType == "artists_with_few_lyrics")
        .ToList();

      cards[0].ArtistName.Should().Be("Zero Lyrics");
      cards[1].ArtistName.Should().Be("One Lyric");
      cards[2].ArtistName.Should().Be("Two Lyrics");
    }

    [Test]
    public async Task should_tag_q1_results_with_artists_with_few_lyrics()
    {
      // arrange
      var artist = CreateApprovedArtist("Tagged Artist", 1);
      Context.Artists.Add(artist);
      Context.ArtistSlugs.Add(CreatePrimarySlug(artist.Id, "tagged-artist"));
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert
      result.OpportunityCards.Should().OnlyContain(c =>
        c.OpportunityType == "artists_with_few_lyrics" ||
        c.OpportunityType == "new_artists_needing_lyrics");
    }

    [Test]
    public async Task should_limit_total_cards_to_max_opportunity_cards()
    {
      // arrange — create 10 artists (more than the limit of 8)
      for (int i = 1; i <= 10; i++)
      {
        var artist = CreateApprovedArtist($"Artist {i}", i, createdAt: DateTime.UtcNow.AddDays(-200));
        Context.Artists.Add(artist);
        Context.ArtistSlugs.Add(CreatePrimarySlug(i, $"artist-{i}"));
      }

      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert
      result.OpportunityCards.Should().HaveCountLessThanOrEqualTo(8);
    }

    [Test]
    public async Task should_include_artist_slug_from_primary_slug()
    {
      // arrange
      var artist = CreateApprovedArtist("Slug Test", 1);
      Context.Artists.Add(artist);
      Context.ArtistSlugs.Add(CreatePrimarySlug(1, "slug-test"));
      Context.ArtistSlugs.Add(new ArtistSlug
      {
        Id = 2,
        ArtistId = 1,
        Name = "alternative-slug",
        IsPrimary = false,
        CreatedAt = DateTime.UtcNow,
      });
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert
      result.OpportunityCards.First().ArtistSlug.Should().Be("slug-test");
    }

    [Test]
    public async Task should_include_has_image_flag()
    {
      // arrange
      var artist = CreateApprovedArtist("Has Image", 1);
      artist.HasImage = true;
      Context.Artists.Add(artist);
      Context.ArtistSlugs.Add(CreatePrimarySlug(1, "has-image"));
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert
      result.OpportunityCards.First().HasImage.Should().BeTrue();
    }

    // --- q2: new artists needing lyrics ---

    [Test]
    public async Task should_return_recent_artists_with_few_lyrics_as_new_artists_needing_lyrics()
    {
      // arrange — artist created within 90 days
      var artist = CreateApprovedArtist("New Artist", 1, createdAt: DateTime.UtcNow.AddDays(-30));
      Context.Artists.Add(artist);
      Context.ArtistSlugs.Add(CreatePrimarySlug(1, "new-artist"));
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert
      result.OpportunityCards.Should().Contain(c =>
        c.ArtistName == "New Artist" &&
        c.OpportunityType == "new_artists_needing_lyrics");
    }

    [Test]
    public async Task should_not_tag_old_artists_as_new_artists_needing_lyrics()
    {
      // arrange — artist created outside 90-day window
      var artist = CreateApprovedArtist("Old Artist", 1, createdAt: DateTime.UtcNow.AddDays(-100));
      Context.Artists.Add(artist);
      Context.ArtistSlugs.Add(CreatePrimarySlug(1, "old-artist"));
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert
      result.OpportunityCards.Should().NotContain(c =>
        c.OpportunityType == "new_artists_needing_lyrics");
    }

    [Test]
    public async Task should_order_q2_by_created_at_descending()
    {
      // arrange — 3 recent artists
      var artist1 = CreateApprovedArtist("Oldest Recent", 1, createdAt: DateTime.UtcNow.AddDays(-60));
      var artist2 = CreateApprovedArtist("Newest Recent", 2, createdAt: DateTime.UtcNow.AddDays(-5));
      var artist3 = CreateApprovedArtist("Middle Recent", 3, createdAt: DateTime.UtcNow.AddDays(-30));

      Context.Artists.AddRange(artist1, artist2, artist3);
      Context.ArtistSlugs.Add(CreatePrimarySlug(1, "oldest-recent"));
      Context.ArtistSlugs.Add(CreatePrimarySlug(2, "newest-recent"));
      Context.ArtistSlugs.Add(CreatePrimarySlug(3, "middle-recent"));
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert
      var q2Cards = result.OpportunityCards
        .Where(c => c.OpportunityType == "new_artists_needing_lyrics")
        .ToList();

      q2Cards[0].ArtistName.Should().Be("Newest Recent");
      q2Cards[1].ArtistName.Should().Be("Middle Recent");
      q2Cards[2].ArtistName.Should().Be("Oldest Recent");
    }

    [Test]
    public async Task should_limit_q2_to_four_cards()
    {
      // arrange — create 6 recent artists
      for (int i = 1; i <= 6; i++)
      {
        var artist = CreateApprovedArtist($"Recent {i}", i, createdAt: DateTime.UtcNow.AddDays(-i));
        Context.Artists.Add(artist);
        Context.ArtistSlugs.Add(CreatePrimarySlug(i, $"recent-{i}"));
      }

      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert
      result.OpportunityCards
        .Count(c => c.OpportunityType == "new_artists_needing_lyrics")
        .Should().BeLessThanOrEqualTo(4);
    }

    // --- composition: q2 + q1, no duplicates ---

    [Test]
    public async Task should_place_q2_results_before_q1_results()
    {
      // arrange
      var recentArtist = CreateApprovedArtist("Recent", 1, createdAt: DateTime.UtcNow.AddDays(-10));
      var oldArtist = CreateApprovedArtist("Old", 2, createdAt: DateTime.UtcNow.AddDays(-200));

      Context.Artists.AddRange(recentArtist, oldArtist);
      Context.ArtistSlugs.Add(CreatePrimarySlug(1, "recent"));
      Context.ArtistSlugs.Add(CreatePrimarySlug(2, "old"));
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert
      var cards = result.OpportunityCards.ToList();
      cards[0].ArtistName.Should().Be("Recent");
      cards[0].OpportunityType.Should().Be("new_artists_needing_lyrics");
      cards[1].ArtistName.Should().Be("Old");
      cards[1].OpportunityType.Should().Be("artists_with_few_lyrics");
    }

    [Test]
    public async Task should_not_duplicate_q2_artists_in_q1_results()
    {
      // arrange — an artist that qualifies for both q1 and q2
      var artist = CreateApprovedArtist("Dual Qualify", 1, createdAt: DateTime.UtcNow.AddDays(-10));
      Context.Artists.Add(artist);
      Context.ArtistSlugs.Add(CreatePrimarySlug(1, "dual-qualify"));
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert — should only appear once (as q2)
      result.OpportunityCards.Count(c => c.ArtistName == "Dual Qualify").Should().Be(1);
      result.OpportunityCards.First(c => c.ArtistName == "Dual Qualify")
        .OpportunityType.Should().Be("new_artists_needing_lyrics");
    }

    [Test]
    public async Task should_return_empty_list_when_no_artists_qualify()
    {
      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert
      result.OpportunityCards.Should().BeEmpty();
    }

    [Test]
    public async Task should_set_is_authenticated_flag()
    {
      // act
      var result = await _service.GetHomepageViewModelAsync(true);

      // assert
      result.IsAuthenticated.Should().BeTrue();
    }

    // --- helpers ---

    private static Artist CreateApprovedArtist(string name, int id, DateTime? createdAt = null)
    {
      return CreateArtist(name, id, isApproved: true, isDeleted: false, createdAt: createdAt);
    }

    private static Artist CreateArtist(
      string name,
      int id,
      bool isApproved = true,
      bool isDeleted = false,
      DateTime? createdAt = null)
    {
      return new Artist
      {
        Id = id,
        FirstName = name,
        LastName = string.Empty,
        FullName = name,
        IsApproved = isApproved,
        IsDeleted = isDeleted,
        HasImage = false,
        Sex = 'm',
        CreatedAt = createdAt ?? DateTime.UtcNow.AddDays(-200),
        UserId = "user-1",
      };
    }

    private static ArtistSlug CreatePrimarySlug(int artistId, string slug)
    {
      return new ArtistSlug
      {
        Id = artistId * 100,
        ArtistId = artistId,
        Name = slug,
        IsPrimary = true,
        CreatedAt = DateTime.UtcNow,
      };
    }

    private static Lyric CreateApprovedLyric(int artistId, string title, int id)
    {
      return CreateLyric(artistId, title, id, isApproved: true, isDeleted: false);
    }

    private static Lyric CreateLyric(
      int artistId,
      string title,
      int id,
      bool isApproved = true,
      bool isDeleted = false)
    {
      return new Lyric
      {
        Id = id,
        ArtistId = artistId,
        Title = title,
        Body = "lyrics body",
        IsApproved = isApproved,
        IsDeleted = isDeleted,
        CreatedAt = DateTime.UtcNow,
        UserId = "user-1",
      };
    }
  }
}
