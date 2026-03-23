namespace Bejebeje.Models.Report;

public class LyricReportViewModel
{
  public int LyricId { get; set; }

  public string LyricTitle { get; set; }

  public string LyricBody { get; set; }

  public string ArtistName { get; set; }

  public string ArtistSlug { get; set; }

  public string LyricSlug { get; set; }

  public bool HasPendingReport { get; set; }
}
