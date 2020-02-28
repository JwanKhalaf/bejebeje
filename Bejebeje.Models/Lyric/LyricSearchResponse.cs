namespace Bejebeje.Models.Lyric
{
  public class LyricSearchResponse
  {
    public string Title { get; set; }

    public string PrimarySlug { get; set; }

    public LyricSearchResponseArtist Artist { get; set; }
  }
}
