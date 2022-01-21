namespace Bejebeje.Common.Extensions
{
  using Enums;

  public static class ImageSizeEnumExtensions
  {
    private const string ExtraSmall = "xsm";

    private const string Small = "sm";

    private const string Standard = "s";

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
