namespace Bejebeje.Services.Services
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Common.Enums;
  using Bejebeje.Common.Extensions;
  using Bejebeje.DataAccess.Context;
  using Bejebeje.Services.Services.Interfaces;
  using Bejebeje.ViewModels.Search;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;

  public class SearchService : ISearchService
  {
    private BbContext context;

    private ILogger logger;

    public SearchService(
      BbContext context,
      ILogger<SearchService> logger)
    {
      this.context = context;
      this.logger = logger;
    }

    public async Task<IList<SearchResultViewModel>> SearchAsync(string searchTerm)
    {
      string searchTermStandardized = searchTerm.Standardize();

      List<SearchResultViewModel> searchResults = new List<SearchResultViewModel>();

      List<SearchResultViewModel> matchedArtists = await context
        .Artists
        .Where(x =>
          EF.Functions.Like(x.FirstName.Standardize(), $"%{searchTermStandardized}%") ||
          EF.Functions.Like(x.LastName.Standardize(), $"%{searchTermStandardized}%") ||
          x.Slugs.Any(s => EF.Functions.Like(s.Name.Standardize(), $"%{searchTermStandardized}%")))
        .Select(x => new SearchResultViewModel
        {
          Name = string.IsNullOrEmpty(x.LastName) ? x.FirstName : $"{x.FirstName} {x.LastName}",
          Slug = x.Slugs.Single(s => s.IsPrimary).Name,
          ImageId = x.Image.Id,
          ResultType = ResultType.Artist
        })
        .ToListAsync();

      List<SearchResultViewModel> matchedLyrics = await context
        .Lyrics
        .Where(x =>
          EF.Functions.Like(x.Title.Standardize(), $"%{searchTermStandardized}%") ||
          x.Slugs.Any(s => EF.Functions.Like(s.Name.Standardize(), $"%{searchTermStandardized}%")))
        .Select(x => new SearchResultViewModel
        {
          Name = x.Title,
          Slug = x.Slugs.Single(s => s.IsPrimary).Name,
          ImageId = 0,
          ResultType = ResultType.Lyric
        })
        .ToListAsync();

      searchResults.AddRange(matchedArtists);
      searchResults.AddRange(matchedLyrics);

      return searchResults;
    }
  }
}
