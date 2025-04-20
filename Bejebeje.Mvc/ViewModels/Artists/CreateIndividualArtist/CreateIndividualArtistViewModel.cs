namespace Bejebeje.Mvc.ViewModels.Artists.CreateIndividualArtist;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

public class CreateIndividualArtistViewModel
{
  [Required] [MaxLength(100)] public string FirstName { get; set; }

  public string LastName { get; set; }

  public string Biography { get; set; }

  public IFormFile Photo { get; set; }

  public string ExistingArtistFullName { get; set; }

  public string ExistingArtistSlug { get; set; }

  public string ExistingArtistImageUrl { get; set; }

  public string ExistingArtistImageAlternateText { get; set; }
}