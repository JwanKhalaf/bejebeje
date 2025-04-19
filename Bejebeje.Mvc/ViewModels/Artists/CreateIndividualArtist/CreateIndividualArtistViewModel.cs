namespace Bejebeje.Mvc.ViewModels.Artists.CreateIndividualArtist;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

public class CreateIndividualArtistViewModel
{
  [Required] [MaxLength(100)] public string FirstName { get; set; }

  public string LastName { get; set; }

  public string Biography { get; set; }

  public string UserId { get; set; }

  public IFormFile Photo { get; set; }
}