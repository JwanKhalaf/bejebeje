namespace Bejebeje.Models.Artist
{
  using System.Collections.Generic;
  using Lyric;

  public class ArtistLyricsViewModel
  {
    public ArtistViewModel Artist { get; set; }

    public string LyricCount { get; set; }

    public IEnumerable<LyricCardViewModel> Lyrics { get; set; }
  }
}
