namespace Bejebeje.Domain
{
  using System;

  public class AuthorImage
  {
    public int Id { get; set; }

    public byte[] Data { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int AuthorId { get; set; }

    public Author Author { get; set; }
  }
}
