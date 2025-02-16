using System.Collections.Generic;

namespace Bejebeje.Models.Author
{
  using System;

  public class AuthorDetailsViewModel
  {
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string FullName { get; set; }

    public string Biography { get; set; }

    public string Slug { get; set; }

    public string ImageUrl { get; set; }

    public string ImageAlternateText { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public IEnumerable<AuthorLyricViewModel> Lyrics { get; set; }
  }

  public class AuthorLyricViewModel
  {
    public string Title { get; set; }

    public string ArtistSlug { get; set; }

    public string LyricSlug { get; set; }
  }
}