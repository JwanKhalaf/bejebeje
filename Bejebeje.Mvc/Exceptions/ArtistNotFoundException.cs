namespace Bejebeje.Mvc.Exceptions
{
  using System;

  public class ArtistNotFoundException : Exception
  {
    public ArtistNotFoundException(string artistSlug)
      : base($"No artist was found matching {artistSlug}")
    {
      ArtistSlug = artistSlug;
    }

    private string ArtistSlug { get; set; }
  }
}
