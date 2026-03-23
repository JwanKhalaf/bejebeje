namespace Bejebeje.Models.Artist
{
  public class ArtistCreationResult
  {
    public int ArtistId { get; set; }

    public string PrimarySlug { get; set; }

    public bool IsSuccessful { get; set; }
  }
}
