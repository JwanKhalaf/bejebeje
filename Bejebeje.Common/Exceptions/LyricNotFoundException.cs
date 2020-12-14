namespace Bejebeje.Common.Exceptions
{
  using System;
  using Extensions;

  public class LyricNotFoundException : Exception
  {
    public LyricNotFoundException(string artistSlug, string lyricSlug)
      : base($"The lyric {lyricSlug.Standardize()} could not be found under the artist {artistSlug.Standardize()}.")
    {
      ArtistSlug = artistSlug;
      LyricSlug = lyricSlug;
    }

    public LyricNotFoundException(int lyricId)
      : base($"No lyric exists with Id: {lyricId}")
    {
      LyricId = lyricId;
    }

    public string ArtistSlug { get; }

    public int LyricId { get; }

    public string LyricSlug { get; }
  }
}
