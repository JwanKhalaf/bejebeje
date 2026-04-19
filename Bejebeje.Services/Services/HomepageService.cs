namespace Bejebeje.Services.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.DataAccess.Context;
  using Bejebeje.Models.Homepage;
  using Bejebeje.Shared.Domain;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Caching.Memory;
  using Microsoft.Extensions.Logging;

  public class HomepageService : IHomepageService
  {
    private readonly BbContext _context;
    private readonly IArtistsService _artistsService;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<HomepageService> _logger;

    public HomepageService(
      BbContext context,
      IArtistsService artistsService,
      IMemoryCache memoryCache,
      ILogger<HomepageService> logger)
    {
      _context = context;
      _artistsService = artistsService;
      _memoryCache = memoryCache;
      _logger = logger;
    }

    public async Task<HomepageViewModel> GetHomepageViewModelAsync(bool isAuthenticated)
    {
      _logger.LogDebug("building homepage view model, authenticated: {IsAuthenticated}", isAuthenticated);

      var opportunityCards = await GetOpportunityCardsAsync();

      var femaleArtists = await _artistsService.GetTopTenFemaleArtistsByLyricsCountAsync();

      var communityImpact = await GetCommunityImpactStatsAsync();

      var viewModel = new HomepageViewModel
      {
        IsAuthenticated = isAuthenticated,
        OpportunityCards = opportunityCards,
        CommunityImpact = communityImpact,
        FemaleArtists = femaleArtists,
      };

      _logger.LogInformation(
        "homepage view model built with {CardCount} opportunity cards",
        opportunityCards.Count);

      return viewModel;
    }

    private async Task<IReadOnlyList<OpportunityCardViewModel>> GetOpportunityCardsAsync()
    {
      _logger.LogDebug("composing opportunity cards");

      // q2 first: new artists needing lyrics (created within recency window)
      var q2Cards = await GetNewArtistsNeedingLyricsAsync();

      var q2ArtistIds = new HashSet<int>(q2Cards.Select(c => c.ArtistId));

      // q1: artists with few lyrics, excluding q2 results
      var q1Cards = await GetArtistsWithFewLyricsAsync(q2ArtistIds);

      // compose: q2 + q1, capped at max
      var composed = q2Cards
        .Concat(q1Cards)
        .Take(HomepageServiceConstants.MaxOpportunityCards)
        .ToList()
        .AsReadOnly();

      _logger.LogDebug(
        "opportunity cards composed: {Q2Count} new artist cards, {Q1Count} few lyrics cards, {TotalCount} total",
        q2Cards.Count,
        q1Cards.Count,
        composed.Count);

      return composed;
    }

    // q1: approved, non-deleted artists with fewer than threshold approved non-deleted lyrics
    private async Task<List<OpportunityCardViewModel>> GetArtistsWithFewLyricsAsync(
      HashSet<int> excludeArtistIds)
    {
      var threshold = HomepageServiceConstants.LyricThreshold;

      var cards = await _context.Artists
        .Where(a => a.IsApproved && !a.IsDeleted)
        .Where(a => !excludeArtistIds.Contains(a.Id))
        .Select(a => new
        {
          a.Id,
          a.FullName,
          a.HasImage,
          PrimarySlug = a.Slugs.Where(s => s.IsPrimary).Select(s => s.Name).FirstOrDefault(),
          ApprovedLyricCount = a.Lyrics.Count(l => l.IsApproved && !l.IsDeleted),
          a.CreatedAt,
        })
        .Where(a => a.ApprovedLyricCount < threshold)
        .OrderBy(a => a.ApprovedLyricCount)
        .ThenByDescending(a => a.CreatedAt)
        .Take(HomepageServiceConstants.MaxOpportunityCards)
        .ToListAsync();

      return cards.Select(a => new OpportunityCardViewModel
      {
        ArtistId = a.Id,
        ArtistName = a.FullName,
        ArtistSlug = a.PrimarySlug,
        HasImage = a.HasImage,
        ApprovedLyricCount = a.ApprovedLyricCount,
        OpportunityType = "artists_with_few_lyrics",
      }).ToList();
    }

    // q2: approved, non-deleted artists created within recency window with few lyrics
    private async Task<List<OpportunityCardViewModel>> GetNewArtistsNeedingLyricsAsync()
    {
      var threshold = HomepageServiceConstants.LyricThreshold;
      var cutoffDate = DateTime.UtcNow.AddDays(-HomepageServiceConstants.RecencyWindowDays);

      var cards = await _context.Artists
        .Where(a => a.IsApproved && !a.IsDeleted)
        .Where(a => a.CreatedAt >= cutoffDate)
        .Select(a => new
        {
          a.Id,
          a.FullName,
          a.HasImage,
          PrimarySlug = a.Slugs.Where(s => s.IsPrimary).Select(s => s.Name).FirstOrDefault(),
          ApprovedLyricCount = a.Lyrics.Count(l => l.IsApproved && !l.IsDeleted),
          a.CreatedAt,
        })
        .Where(a => a.ApprovedLyricCount < threshold)
        .OrderByDescending(a => a.CreatedAt)
        .Take(HomepageServiceConstants.MaxNewArtistCards)
        .ToListAsync();

      return cards.Select(a => new OpportunityCardViewModel
      {
        ArtistId = a.Id,
        ArtistName = a.FullName,
        ArtistSlug = a.PrimarySlug,
        HasImage = a.HasImage,
        ApprovedLyricCount = a.ApprovedLyricCount,
        OpportunityType = "new_artists_needing_lyrics",
      }).ToList();
    }

    // q3: community impact stats with caching
    private async Task<CommunityImpactStatsViewModel> GetCommunityImpactStatsAsync()
    {
      if (_memoryCache.TryGetValue(
        HomepageServiceConstants.StatsCacheKey,
        out CommunityImpactStatsViewModel cached))
      {
        _logger.LogDebug("community impact stats served from cache");
        return cached;
      }

      _logger.LogDebug("community impact stats cache miss, querying database");

      try
      {
        var totalApprovedLyrics = await _context.Lyrics
          .CountAsync(l => l.IsApproved && !l.IsDeleted);

        var totalApprovedArtists = await _context.Artists
          .CountAsync(a => a.IsApproved && !a.IsDeleted);

        var totalContributors = await _context.PointEvents
          .Where(pe =>
            pe.ActionType == PointActionType.ArtistApproved ||
            pe.ActionType == PointActionType.LyricApproved)
          .Select(pe => pe.UserId)
          .Distinct()
          .CountAsync();

        var stats = new CommunityImpactStatsViewModel
        {
          TotalApprovedLyrics = totalApprovedLyrics,
          TotalApprovedArtists = totalApprovedArtists,
          TotalContributors = totalContributors,
        };

        var cacheOptions = new MemoryCacheEntryOptions()
          .SetAbsoluteExpiration(TimeSpan.FromMinutes(HomepageServiceConstants.StatsCacheTtlMinutes));

        _memoryCache.Set(HomepageServiceConstants.StatsCacheKey, stats, cacheOptions);

        _logger.LogInformation(
          "community impact stats computed: {Lyrics} lyrics, {Artists} artists, {Contributors} contributors",
          totalApprovedLyrics,
          totalApprovedArtists,
          totalContributors);

        return stats;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "failed to compute community impact stats, returning null");
        return null;
      }
    }
  }
}
