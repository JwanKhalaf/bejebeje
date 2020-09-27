namespace Bejebeje.Domain
{
  using System;
  using Interfaces;

  public class LyricSlug : IBaseEntity
  {
    public int Id { get; set; }

    public string Name { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool IsDeleted { get; set; }

    public int LyricId { get; set; }
  }
}
