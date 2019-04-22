namespace Bejebeje.Domain
{
  using System;
  using System.Collections.Generic;
  using Bejebeje.Domain.Interfaces;

  public class Artist : IBaseEntity, IApprovable
  {
    public int Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public IEnumerable<ArtistSlug> Slugs { get; set; }

    public string ImageUrl { get; set; }

    public bool IsApproved { get; set; }

    public string UserId { get; set; }

    public IEnumerable<Lyric> Lyrics { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }

    public bool IsDeleted { get; set; }
  }
}
