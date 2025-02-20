namespace Bejebeje.Common.Helpers
{
  using Enums;
  using Extensions;

  public static class ImageUrlBuilder
  {
    public static string BuildArtistImageUrl(
      bool hasImage,
      int artistId,
      ImageSize imageSize)
    {
      return hasImage
        ? BuildArtistS3ImageUrl(artistId, imageSize)
        : GetPlaceholderImageUrl(imageSize);
    }
    
    public static string BuildAuthorImageUrl(
      bool hasImage,
      int authorId,
      ImageSize imageSize)
    {
      return hasImage
        ? BuildAuthorS3ImageUrl(authorId, imageSize)
        : GetPlaceholderImageUrl(imageSize);
    }

    public static string GetArtistImageAlternateText(
      bool hasImage,
      string artistFullName)
    {
      return hasImage
        ? $"a photo of {artistFullName}"
        : "the bejebeje logo used as a placeholder because the artist has no photo";
    }
    
    public static string GetAuthorImageAlternateText(
      bool hasImage,
      string authorFullName)
    {
      return hasImage
        ? $"a photo of {authorFullName}"
        : "the bejebeje logo used as a placeholder because the author has no photo";
    }

    private static string BuildArtistS3ImageUrl(
      int artistId,
      ImageSize imageSize)
    {
      string size = imageSize.GetCorrespondingFolder();

      return $"https://s3.eu-west-2.amazonaws.com/bejebeje.com/artist-images/{artistId}-{size}";
    }
    
    private static string BuildAuthorS3ImageUrl(
      int authorId,
      ImageSize imageSize)
    {
      string size = imageSize.GetCorrespondingFolder();

      return $"https://s3.eu-west-2.amazonaws.com/bejebeje.com/author-images/{authorId}-{size}";
    }

    private static string GetPlaceholderImageUrl(
      ImageSize imageSize)
    {
      string size = imageSize.GetCorrespondingFolder();

      return $"https://s3.eu-west-2.amazonaws.com/bejebeje.com/artist-images/placeholder-{size}";
    }
  }
}
