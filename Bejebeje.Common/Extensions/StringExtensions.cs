namespace Bejebeje.Common.Extensions
{
  using System;
  using System.Globalization;
  using System.Linq;
  using System.Text;
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

    public static string NormalizeStringForUrl(this string name)
    {
      string lowerCaseName = name.Trim().ToLowerInvariant();
      String normalizedString = lowerCaseName.Normalize(NormalizationForm.FormD);
      StringBuilder stringBuilder = new StringBuilder();

      foreach (char c in normalizedString)
      {
        switch (CharUnicodeInfo.GetUnicodeCategory(c))
        {
          case UnicodeCategory.LowercaseLetter:
          case UnicodeCategory.UppercaseLetter:
          case UnicodeCategory.DecimalDigitNumber:
            stringBuilder.Append(c);
            break;
          case UnicodeCategory.SpaceSeparator:
          case UnicodeCategory.ConnectorPunctuation:
          case UnicodeCategory.DashPunctuation:
            stringBuilder.Append('_');
            break;
        }
      }
      string result = stringBuilder.ToString();
      return String.Join("-", result.Split(new char[] { '_' }
        , StringSplitOptions.RemoveEmptyEntries));
    }

    public static string FirstCharToUpper(this string input) =>
      input switch
      {
        null => throw new ArgumentNullException(nameof(input)),
        "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
        _ => input.First().ToString().ToUpper() + input.Substring(1)
      };
    
    public static string TruncateLongString(this string str, int maxLength)
    {
      if (string.IsNullOrEmpty(str) || str.Length <= maxLength) return str;
      
      return str.Substring(0, Math.Min(str.Length, maxLength)) + "…";
    }
  }
}
