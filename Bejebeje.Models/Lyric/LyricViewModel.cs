namespace Bejebeje.Models.Lyric
{
  using System;

  public class LyricViewModel
  {
    public string Title { get; set; }

    public string Body { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }
  }
}
