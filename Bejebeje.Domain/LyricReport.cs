namespace Bejebeje.Domain
{
  using System;
  using Interfaces;

  public class LyricReport : IBaseEntity
  {
    public int Id { get; set; }

    public int LyricId { get; set; }

    public string UserId { get; set; }

    public int Category { get; set; }

    public string Comment { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool IsDeleted { get; set; }

    public string ActionedBy { get; set; }

    public DateTime? ActionedAt { get; set; }
  }
}
