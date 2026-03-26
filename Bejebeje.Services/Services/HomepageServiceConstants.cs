namespace Bejebeje.Services.Services
{
  public static class HomepageServiceConstants
  {
    // minimum number of approved lyrics before an artist is excluded from opportunity cards
    public const int LyricThreshold = 3;

    // number of days to look back for "new" artists
    public const int RecencyWindowDays = 90;

    // maximum total opportunity cards returned
    public const int MaxOpportunityCards = 8;

    // maximum cards from the "new artists needing lyrics" query
    public const int MaxNewArtistCards = 4;

    // community impact stats cache duration in minutes
    public const int StatsCacheTtlMinutes = 30;

    // cache key for community impact stats
    public const string StatsCacheKey = "homepage:community_impact_stats";
  }
}
