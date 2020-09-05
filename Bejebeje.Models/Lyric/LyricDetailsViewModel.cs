namespace Bejebeje.Models.Lyric
{
  using System;
  using Bejebeje.Models.Artist;

  public class LyricDetailsViewModel
  {
    public string Title { get; set; }

    public string Body { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public ArtistViewModel Artist { get; set; }
  }
}
