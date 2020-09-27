namespace Bejebeje.Models.Author
{
  using System;

  public class AuthorDetailsResponse
  {
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Biography { get; set; }

    public string Slug { get; set; }

    public bool HasImage { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }
  }
}
