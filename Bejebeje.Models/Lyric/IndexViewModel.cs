namespace Bejebeje.Models.Lyric
{
  using System.Collections.Generic;
  using Artist;

  public class IndexViewModel
  {
    public IEnumerable<LyricItemViewModel> RecentlyVerifiedLyrics { get; set; }

    public IEnumerable<LyricItemViewModel> RecentlySubmittedLyrics { get; set; }

    public IEnumerable<RandomFemaleArtistItemViewModel> FemaleArtists { get; set; }
  }
}