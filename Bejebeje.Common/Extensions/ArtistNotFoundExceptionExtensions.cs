namespace Bejebeje.Common.Extensions
{
  using Bejebeje.Common.Exceptions;

  public static class ArtistNotFoundExceptionExtensions
  {
    public static string ToLogData(this ArtistNotFoundException model)
    {
      return $@"ArtistId: {model.ArtistId}, ArtistSlug: {model.ArtistSlug}.";
    }
  }
}
