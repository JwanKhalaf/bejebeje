namespace Bejebeje.Domain
{
  using System;

  public class PointEvent
  {
    public int Id { get; set; }

    public int UserId { get; set; }

    public PointActionType ActionType { get; set; }

    public int Points { get; set; }

    public int EntityId { get; set; }

    public string EntityName { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; }
  }
}
