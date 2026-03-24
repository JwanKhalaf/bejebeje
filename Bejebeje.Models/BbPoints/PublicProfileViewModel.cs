namespace Bejebeje.Models.BbPoints
{
  public class PublicProfileViewModel
  {
    public string Username { get; set; }

    public string CognitoUserId { get; set; }

    public int TotalPoints { get; set; }

    public string ContributorLabel { get; set; }

    public int ArtistsSubmittedCount { get; set; }

    public int LyricsSubmittedCount { get; set; }
  }
}
