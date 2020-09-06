namespace Bejebeje.Models.Search
{
  using System.Collections.Generic;

  public class SearchViewModel
  {
    public SearchViewModel()
    {
      Lyrics = new List<SearchLyricResultViewModel>();
      Artists = new List<SearchArtistResultViewModel>();
    }

    public string SearchTerm { get; set; }

    public IEnumerable<SearchArtistResultViewModel> Artists { get; set; }

    public IEnumerable<SearchLyricResultViewModel> Lyrics { get; set; }
  }
}
