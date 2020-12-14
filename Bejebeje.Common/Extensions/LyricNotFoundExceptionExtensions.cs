namespace Bejebeje.Common.Extensions
{
  using Exceptions;

  public static class LyricNotFoundExceptionExtensions
  {
    public static string ToLogData(this LyricNotFoundException model)
    {
      return $@"ArtistSlug: {model.ArtistSlug}, LyricSlug: {model.LyricSlug}, LyricId: {model.LyricId}.";
    }
  }
}
