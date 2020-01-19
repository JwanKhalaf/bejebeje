namespace Bejebeje.Models.Lyric
{
  using System;

  public class LyricResponse
  {
    public string Title { get; set; }

    public string Body { get; set; }

    public string AuthorSlug { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }
  }
}
