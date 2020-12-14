namespace Bejebeje.Common.Extensions
{
  using Exceptions;

  public static class ArtistNotFoundExceptionExtensions
  {
    public static string ToLogData(this ArtistNotFoundException model)
    {
      return $@"ArtistId: {model.ArtistId}, ArtistSlug: {model.ArtistSlug}.";
    }
  }
}
