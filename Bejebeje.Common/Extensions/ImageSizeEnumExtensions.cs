namespace Bejebeje.Common.Extensions
{
  using Enums;

  public static class ImageSizeEnumExtensions
  {
    private const string ExtraSmall = "extra-small";

    private const string Small = "small";

    private const string Standard = "standard";

    public static string GetCorrespondingFolder(this ImageSize imageSize)
    {
      return imageSize switch
      {
        ImageSize.ExtraSmall => ExtraSmall,
        ImageSize.Small => Small,
        _ => Standard
      };
    }
  }
}
