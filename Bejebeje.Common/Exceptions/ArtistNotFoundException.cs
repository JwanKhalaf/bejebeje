namespace Bejebeje.Common.Exceptions
{
  using System;

  public class ArtistNotFoundException : Exception
  {
    public ArtistNotFoundException(int artistId)
      : base ($"The artist with Id: {artistId} could not be found.")
    {
      ArtistId = artistId;
    }

    public ArtistNotFoundException(string artistSlug)
      : base($"No artist could be found with a slug matching: {artistSlug}.")
    {
      ArtistSlug = artistSlug;
    }

    private ArtistNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public int ArtistId { get; }

    public string ArtistSlug { get; }
  }
}
