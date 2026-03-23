namespace Bejebeje.Models.Report;

using System.ComponentModel.DataAnnotations;

public class LyricReportFormViewModel
{
  [Required]
  public int LyricId { get; set; }

  [Required]
  [Range(0, 4)]
  public int? Category { get; set; }

  [MaxLength(2000)]
  public string Comment { get; set; }

  [Required]
  public string ArtistSlug { get; set; }

  [Required]
  public string LyricSlug { get; set; }
}
