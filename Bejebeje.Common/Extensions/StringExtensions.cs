namespace Bejebeje.Common.Extensions
{
  using System.Text.RegularExpressions;

  public static class StringExtensions
  {
    public static string ToSnakeCase(this string input)
    {
      if (string.IsNullOrEmpty(input)) { return input; }

      var startUnderscores = Regex.Match(input, @"^_+");
      return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }

    public static string Standardize(this string input)
    {
      if (!string.IsNullOrEmpty(input))
      {
        return input.Trim().ToLowerInvariant();
      }

      return string.Empty;
    }
  }
}
