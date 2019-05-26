namespace Bejebeje.ViewModels.Search
{
  using Bejebeje.Common.Enums;

  public class SearchResultViewModel
  {
    public string Slug { get; set; }

    public string Name { get; set; }

    public int ImageId { get; set; }

    public ResultType ResultType { get; set; }
  }
}
