namespace Bejebeje.Common.Exceptions
{
  using System;

  public class ArtistExistsException : Exception
  {
    public ArtistExistsException(string artistSlug)
      : base($"The artist with the slug [{artistSlug}] already exists.")
    {
      ArtistSlug = artistSlug;
    }

    public ArtistExistsException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public string ArtistSlug { get; set; }
  }
}
