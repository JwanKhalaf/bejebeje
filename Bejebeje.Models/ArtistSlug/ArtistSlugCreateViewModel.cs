namespace Bejebeje.Models.ArtistSlug
{
  using System;

  public class ArtistSlugCreateViewModel
  {
    public string Name { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public int ArtistId { get; set; }
  }
}
