namespace Bejebeje.Common.Exceptions
{
  using System;

  public class AuthorNotFoundException : Exception
  {
    public AuthorNotFoundException(string authorSlug)
      : base($"Not author can be found with a slug matching: {authorSlug}.")
    {
      AuthorSlug = authorSlug;
    }

    public AuthorNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public string AuthorSlug { get; set; }
  }
}
