namespace Bejebeje.Domain
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using Interfaces;

  public class LyricReport : IBaseEntity
  {
    public int Id { get; set; }

    public int LyricId { get; set; }

    [Required]
    public string UserId { get; set; }

    public int Category { get; set; }

    [MaxLength(2000)]
    public string Comment { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool IsDeleted { get; set; }

    public string ActionedBy { get; set; }

    public DateTime? ActionedAt { get; set; }
  }
}
