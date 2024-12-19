﻿namespace Bejebeje.Models.Artist
{
  using System;

  public class ArtistViewModel
  {
    public int Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string FullName { get; set; }

    public string PrimarySlug { get; set; }

    public bool IsApproved { get; set; }

    public string ImageUrl { get; set; }

    public string ImageAlternateText { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool IsOwnSubmission { get; set; }

    public bool IsEditable()
    {
      return IsOwnSubmission && (DateTime.UtcNow - CreatedAt).TotalDays < 2 && !IsApproved;
    }
  }
}
