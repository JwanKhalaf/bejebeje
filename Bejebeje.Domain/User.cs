namespace Bejebeje.Domain
{
  using System;

  public class User
  {
    public int Id { get; set; }

    public string CognitoUserId { get; set; }

    public string Username { get; set; }

    public int ArtistSubmissionPoints { get; set; }

    public int ArtistApprovalPoints { get; set; }

    public int LyricSubmissionPoints { get; set; }

    public int LyricApprovalPoints { get; set; }

    public int ReportSubmissionPoints { get; set; }

    public int ReportAcknowledgementPoints { get; set; }

    public int LastSeenPoints { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }
  }
}
