namespace Bejebeje.Models.Lyric;

using System;
using Artist;
using Author;

public class LyricDetailsViewModel
{
  public int Id { get; set; }

  public string Title { get; set; }

  public string Body { get; set; }

  public int NumberOfLikes { get; set; }

  public bool AlreadyLiked { get; set; }

  public string PrimarySlug { get; set; }

  public bool IsApproved { get; set; }

  public bool IsVerified { get; set; }

  public DateTime CreatedAt { get; set; }

  public DateTime? ModifiedAt { get; set; }

  public ArtistViewModel Artist { get; set; }

  public AuthorViewModel Author { get; set; }

  public string SubmitterUsername { get; set; }
}