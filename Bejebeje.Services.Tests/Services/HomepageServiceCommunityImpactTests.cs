namespace Bejebeje.Services.Tests.Services
{
  using System;
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
  public class HomepageServiceCommunityImpactTests : DatabaseTestBase
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

    // --- q3: community impact stats ---

    [Test]
    public async Task should_count_approved_non_deleted_lyrics()
    {
      // arrange
      var artist = CreateApprovedArtist(1);
      Context.Artists.Add(artist);

      Context.Lyrics.Add(CreateLyric(1, 1, isApproved: true, isDeleted: false));
      Context.Lyrics.Add(CreateLyric(1, 2, isApproved: true, isDeleted: false));
      Context.Lyrics.Add(CreateLyric(1, 3, isApproved: false, isDeleted: false));
      Context.Lyrics.Add(CreateLyric(1, 4, isApproved: true, isDeleted: true));
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert — only 2 approved non-deleted lyrics
      result.CommunityImpact.Should().NotBeNull();
      result.CommunityImpact.TotalApprovedLyrics.Should().Be(2);
    }

    [Test]
    public async Task should_count_approved_non_deleted_artists()
    {
      // arrange
      Context.Artists.Add(CreateArtist(1, isApproved: true, isDeleted: false));
      Context.Artists.Add(CreateArtist(2, isApproved: true, isDeleted: false));
      Context.Artists.Add(CreateArtist(3, isApproved: false, isDeleted: false));
      Context.Artists.Add(CreateArtist(4, isApproved: true, isDeleted: true));
      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert — only 2 approved non-deleted artists
      result.CommunityImpact.Should().NotBeNull();
      result.CommunityImpact.TotalApprovedArtists.Should().Be(2);
    }

    [Test]
    public async Task should_count_distinct_contributors_from_point_events()
    {
      // arrange
      var user1 = new User { Id = 1, CognitoUserId = "u1", Username = "user1", CreatedAt = DateTime.UtcNow };
      var user2 = new User { Id = 2, CognitoUserId = "u2", Username = "user2", CreatedAt = DateTime.UtcNow };
      var user3 = new User { Id = 3, CognitoUserId = "u3", Username = "user3", CreatedAt = DateTime.UtcNow };

      Context.Users.AddRange(user1, user2, user3);

      // user1: artist approved (action_type 2) — counts
      Context.PointEvents.Add(new PointEvent
      {
        Id = 1,
        UserId = 1,
        ActionType = PointActionType.ArtistApproved,
        Points = 10,
        EntityId = 1,
        EntityName = "artist",
        CreatedAt = DateTime.UtcNow,
      });

      // user2: lyric approved (action_type 4) — counts
      Context.PointEvents.Add(new PointEvent
      {
        Id = 2,
        UserId = 2,
        ActionType = PointActionType.LyricApproved,
        Points = 10,
        EntityId = 1,
        EntityName = "lyric",
        CreatedAt = DateTime.UtcNow,
      });

      // user3: lyric submitted (action_type 3) — does NOT count
      Context.PointEvents.Add(new PointEvent
      {
        Id = 3,
        UserId = 3,
        ActionType = PointActionType.LyricSubmitted,
        Points = 5,
        EntityId = 1,
        EntityName = "lyric",
        CreatedAt = DateTime.UtcNow,
      });

      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert — only user1 and user2 count (action types 2 and 4)
      result.CommunityImpact.Should().NotBeNull();
      result.CommunityImpact.TotalContributors.Should().Be(2);
    }

    [Test]
    public async Task should_count_contributor_only_once_even_with_multiple_events()
    {
      // arrange
      var user = new User { Id = 1, CognitoUserId = "u1", Username = "user1", CreatedAt = DateTime.UtcNow };
      Context.Users.Add(user);

      Context.PointEvents.Add(new PointEvent
      {
        Id = 1,
        UserId = 1,
        ActionType = PointActionType.ArtistApproved,
        Points = 10,
        EntityId = 1,
        EntityName = "artist",
        CreatedAt = DateTime.UtcNow,
      });

      Context.PointEvents.Add(new PointEvent
      {
        Id = 2,
        UserId = 1,
        ActionType = PointActionType.LyricApproved,
        Points = 10,
        EntityId = 2,
        EntityName = "lyric",
        CreatedAt = DateTime.UtcNow,
      });

      await Context.SaveChangesAsync();

      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert — same user counted once
      result.CommunityImpact.TotalContributors.Should().Be(1);
    }

    [Test]
    public async Task should_return_zero_counts_when_no_data_exists()
    {
      // act
      var result = await _service.GetHomepageViewModelAsync(false);

      // assert
      result.CommunityImpact.Should().NotBeNull();
      result.CommunityImpact.TotalApprovedLyrics.Should().Be(0);
      result.CommunityImpact.TotalApprovedArtists.Should().Be(0);
      result.CommunityImpact.TotalContributors.Should().Be(0);
    }

    // --- caching ---

    [Test]
    public async Task should_cache_community_impact_stats()
    {
      // arrange
      Context.Artists.Add(CreateArtist(1, isApproved: true, isDeleted: false));
      await Context.SaveChangesAsync();

      // act — first call populates cache
      var result1 = await _service.GetHomepageViewModelAsync(false);

      // add another artist — should not affect cached result
      Context.Artists.Add(CreateArtist(2, isApproved: true, isDeleted: false));
      await Context.SaveChangesAsync();

      var result2 = await _service.GetHomepageViewModelAsync(false);

      // assert — second call returns cached value (still 1)
      result2.CommunityImpact.TotalApprovedArtists.Should().Be(1);
    }

    [Test]
    public async Task should_use_correct_cache_key()
    {
      // act
      await _service.GetHomepageViewModelAsync(false);

      // assert
      _memoryCache.TryGetValue(
        HomepageServiceConstants.StatsCacheKey,
        out object cached).Should().BeTrue();
    }

    // --- helpers ---

    private static Artist CreateApprovedArtist(int id)
    {
      return CreateArtist(id, isApproved: true, isDeleted: false);
    }

    private static Artist CreateArtist(int id, bool isApproved, bool isDeleted)
    {
      return new Artist
      {
        Id = id,
        FirstName = $"Artist {id}",
        LastName = string.Empty,
        FullName = $"Artist {id}",
        IsApproved = isApproved,
        IsDeleted = isDeleted,
        HasImage = false,
        Sex = 'm',
        CreatedAt = DateTime.UtcNow.AddDays(-200),
        UserId = "user-1",
      };
    }

    private static Lyric CreateLyric(int artistId, int id, bool isApproved, bool isDeleted)
    {
      return new Lyric
      {
        Id = id,
        ArtistId = artistId,
        Title = $"Song {id}",
        Body = "lyrics body",
        IsApproved = isApproved,
        IsDeleted = isDeleted,
        CreatedAt = DateTime.UtcNow,
        UserId = "user-1",
      };
    }
  }
}
