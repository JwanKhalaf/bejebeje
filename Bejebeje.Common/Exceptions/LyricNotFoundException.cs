namespace Bejebeje.Common.Exceptions
{
  using System;
  using Bejebeje.Common.Extensions;

  public class LyricNotFoundException : Exception
  {
    public LyricNotFoundException(string artistSlug, string lyricSlug)
      : base($"The lyric {lyricSlug.Standardize()} could not be found under the artist {artistSlug.Standardize()}.")
    {
      ArtistSlug = artistSlug;
      LyricSlug = lyricSlug;
    }

    private LyricNotFoundException(string message) : base(message)
    {
    }

    private LyricNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public string ArtistSlug { get; }

    public int LyricId { get; }

    public string LyricSlug { get; }
  }
}
