namespace Bejebeje.Services.Services.Interfaces
{
  using System.Collections.Generic;
  using Bejebeje.ViewModels.Lyric;

  public interface ILyricsService
  {
    IList<LyricCardViewModel> GetLyricsByArtistSlug(string artistSlug);
  }
}
