using System;

namespace Bejebeje.Common.Exceptions
{
  public class MissingArtistImageException : Exception
  {
    public MissingArtistImageException(string artistSlug)
      : base($"No artist could be found with a slug matching: {artistSlug}.")
    {
      ArtistSlug = artistSlug;
    }

    private MissingArtistImageException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public string ArtistSlug { get; }
  }
}
