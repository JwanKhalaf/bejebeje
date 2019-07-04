namespace Bejebeje.Domain
{
  using System;
  using Bejebeje.Domain.Interfaces;

  public class AuthorSlug : IBaseEntity
  {
    public int Id { get; set; }

    public string Name { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }

    public bool IsDeleted { get; set; }

    public int AuthorId { get; set; }
  }
}
