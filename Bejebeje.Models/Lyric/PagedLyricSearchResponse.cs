namespace Bejebeje.Models.Lyric
{
  using System.Collections.Generic;
  using Bejebeje.Models.Paging;

  public class PagedLyricSearchResponse
  {
    public ICollection<LyricSearchResponse> Lyrics { get; set; } = new List<LyricSearchResponse>();

    public PagingResponse Paging { get; set; }
  }
}
