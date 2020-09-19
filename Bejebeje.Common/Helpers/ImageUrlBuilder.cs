namespace Bejebeje.Common.Helpers
{
  using Enums;
  using Extensions;

  public static class ImageUrlBuilder
  {
    public static string BuildImageUrl(
      bool hasImage,
      string artistPrimarySlug,
      int artistId,
      ImageSize imageSize)
    {
      return hasImage
        ? BuildS3ImageUrl(artistPrimarySlug, artistId, imageSize)
        : GetPlaceholderImageUrl(imageSize);
    }

    public static string GetImageAlternateText(
      bool hasImage,
      string artistFullName)
    {
      return hasImage
        ? $"a photo of {artistFullName}"
        : "the bejebeje logo used as a placeholder because the artist has no photo";
    }

    private static string BuildS3ImageUrl(
      string artistPrimarySlug,
      int artistId,
      ImageSize imageSize)
    {
      string sizeFolder = imageSize.GetCorrespondingFolder();

      return $"https://s3.eu-west-2.amazonaws.com/bejebeje.com/artist-images/{sizeFolder}/{artistPrimarySlug}-{artistId}";
    }

    private static string GetPlaceholderImageUrl(
      ImageSize imageSize)
    {
      string sizeFolder = imageSize.GetCorrespondingFolder();

      return $"https://s3.eu-west-2.amazonaws.com/bejebeje.com/artist-images/{sizeFolder}/artist-placeholder";
    }
  }
}
