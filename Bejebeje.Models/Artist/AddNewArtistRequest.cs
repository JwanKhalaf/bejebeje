namespace Bejebeje.Models.Artist
{
  using System.ComponentModel.DataAnnotations;

  public class AddNewArtistRequest
  {
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; }

    public string LastName { get; set; }
  }
}
