namespace Bejebeje.Models.Artist
{
  using System.ComponentModel.DataAnnotations;

  public class CreateArtistViewModel
  {
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string UserId { get; set; }
  }
}
