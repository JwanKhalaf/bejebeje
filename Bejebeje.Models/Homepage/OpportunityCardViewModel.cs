namespace Bejebeje.Models.Homepage
{
  public class OpportunityCardViewModel
  {
    public int ArtistId { get; set; }

    public string ArtistName { get; set; }

    public string ArtistSlug { get; set; }

    public bool HasImage { get; set; }

    public int ApprovedLyricCount { get; set; }

    public string OpportunityType { get; set; }
  }
}
