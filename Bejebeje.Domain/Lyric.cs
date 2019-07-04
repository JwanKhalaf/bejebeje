namespace Bejebeje.Domain
{
  using System;
  using System.Collections.Generic;
  using Bejebeje.Domain.Interfaces;

  public class Lyric : IBaseEntity, IApprovable
  {
    public int Id { get; set; }

    public string Title { get; set; }

    public string Body { get; set; }

    public string UserId { get; set; }

    public IEnumerable<LyricSlug> Slugs { get; set; } = new List<LyricSlug>();

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsApproved { get; set; }

    public int ArtistId { get; set; }

    public Artist Artist { get; set; }

    public int? AuthorId { get; set; }

    public Author Author { get; set; }
  }
}
