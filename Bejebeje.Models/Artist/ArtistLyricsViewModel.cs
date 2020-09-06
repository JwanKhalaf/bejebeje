namespace Bejebeje.Models.Artist
{
  using System.Collections.Generic;
  using Bejebeje.Models.Lyric;

  public class ArtistLyricsViewModel
  {
    public ArtistViewModel Artist { get; set; }

    public IEnumerable<LyricCardViewModel> Lyrics { get; set; }
  }
}
