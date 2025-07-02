namespace Bejebeje.Mvc.ViewModels.Artists.CreateBandArtist;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

public class CreateBandArtistViewModel
{
  [Required(ErrorMessage = "Please enter the band's name")]
  [Display(Name = "Band name")]
  public string BandName { get; set; }

  public string Biography { get; set; }

  public IFormFile Photo { get; set; }

  public string ExistingArtistFullName { get; set; }

  public string ExistingArtistSlug { get; set; }

  public string ExistingArtistImageUrl { get; set; }

  public string ExistingArtistImageAlternateText { get; set; }
}