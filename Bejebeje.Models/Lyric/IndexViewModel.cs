namespace Bejebeje.Models.Lyric
{
  using System.Collections.Generic;
  using Artist;

  public class IndexViewModel
  {
    public IEnumerable<LyricItemViewModel> Lyrics { get; set; }

    public IEnumerable<RandomFemaleArtistItemViewModel> FemaleArtists { get; set; }
  }
}
