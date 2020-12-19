namespace Bejebeje.Models.Lyric
{
  using System;
  using Artist;

  public class LyricDetailsViewModel
  {
    public int Id { get; set; }

    public string Title { get; set; }

    public string Body { get; set; }

    public int NumberOfLikes { get; set; }

    public bool AlreadyLiked { get; set; }

    public string PrimarySlug { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public ArtistViewModel Artist { get; set; }
  }
}
