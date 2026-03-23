namespace Bejebeje.Models.Artist;

using System.ComponentModel.DataAnnotations;

public class CreateIndividualArtistDto
{
  [Required] [MaxLength(100)] public string FirstName { get; set; }

  public string LastName { get; set; }

  public string Biography { get; set; }

  public string UserId { get; set; }

  public byte[] Photo { get; set; }
}