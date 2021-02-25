namespace Bejebeje.Domain
{
  using System;
  using System.Collections.Generic;
  using Interfaces;

  public class Author : IBaseEntity, IApprovable
  {
    public int Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string FullName { get; set; }

    public string Biography { get; set; }

    public IEnumerable<AuthorSlug> Slugs { get; set; } = new List<AuthorSlug>();

    public IEnumerable<Lyric> Lyrics { get; set; }

    public bool IsApproved { get; set; }

    public string UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool IsDeleted { get; set; }

    public bool HasImage { get; set; }

    public char Sex { get; set; }
  }
}
