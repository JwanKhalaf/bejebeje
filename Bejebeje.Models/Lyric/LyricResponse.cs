namespace Bejebeje.Models.Lyric
{
  using Bejebeje.Models.Author;

  public class LyricResponse
  {
    public string Title { get; set; }

    public string Body { get; set; }

    public LyricAuthorResponse Author { get; set; }
  }
}
