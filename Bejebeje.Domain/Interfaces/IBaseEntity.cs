namespace Bejebeje.Domain.Interfaces
{
  using System;

  public interface IBaseEntity
  {
    int Id { get; set; }

    DateTime CreatedAt { get; set; }

    DateTime? ModifiedAt { get; set; }

    bool IsDeleted { get; set; }
  }
}
