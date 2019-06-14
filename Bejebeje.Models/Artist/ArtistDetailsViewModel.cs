namespace Bejebeje.Models.Artist
{
  using System;

  public class ArtistDetailsViewModel
  {
    public int Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Slug { get; set; }

    public int ImageId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }
  }
}
