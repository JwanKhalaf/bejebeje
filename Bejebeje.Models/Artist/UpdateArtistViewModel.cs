namespace Bejebeje.Models.Artist
{
  using System.ComponentModel.DataAnnotations;

  public class UpdateArtistViewModel
  {
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; }

    public string LastName { get; set; }
  }
}
