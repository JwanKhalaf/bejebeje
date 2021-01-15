namespace Bejebeje.Models.Lyric
{
  using Artist;

  public class CreateLyricViewModel
  {
    public ArtistViewModel Artist { get; set; }

    public string Title { get; set; }

    public string Body { get; set; }

    public string UserId { get; set; }
  }
}
