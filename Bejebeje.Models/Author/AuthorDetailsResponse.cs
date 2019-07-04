namespace Bejebeje.Models.Author
{
  using System;

  public class AuthorDetailsResponse
  {
    public int Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Biography { get; set; }

    public string Slug { get; set; }

    public int ImageId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }
  }
}
