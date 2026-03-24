namespace Bejebeje.Models.BbPoints
{
  using System.Collections.Generic;

  public class OwnProfileViewModel
  {
    public int ArtistSubmissionPoints { get; set; }

    public int ArtistApprovalPoints { get; set; }

    public int LyricSubmissionPoints { get; set; }

    public int LyricApprovalPoints { get; set; }

    public int ReportSubmissionPoints { get; set; }

    public int ReportAcknowledgementPoints { get; set; }

    public int LikePoints { get; set; }

    public int TotalPoints { get; set; }

    public string ContributorLabel { get; set; }

    public string Username { get; set; }

    public List<PointActivityViewModel> RecentActivity { get; set; } = new List<PointActivityViewModel>();
  }
}
