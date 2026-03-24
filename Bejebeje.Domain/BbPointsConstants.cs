namespace Bejebeje.Domain
{
  public static class BbPointsConstants
  {
    public const int ArtistSubmittedNoPhoto = 1;

    public const int ArtistSubmittedWithPhoto = 5;

    public const int ArtistApprovedNoPhoto = 9;

    public const int ArtistApprovedWithPhoto = 10;

    public const int LyricSubmitted = 5;

    public const int LyricApproved = 15;

    public const int ReportSubmitted = 1;

    public const int ReportAcknowledged = 4;

    public const int LikesPerPoint = 10;

    public static string GetContributorLabel(int totalPoints)
    {
      return totalPoints switch
      {
        >= 500 => "Top Contributor",
        >= 200 => "Regular Contributor",
        >= 50 => "Contributor",
        _ => "New Contributor",
      };
    }
  }
}
