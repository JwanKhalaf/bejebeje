namespace Bejebeje.Services.Services
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Common.Extensions;
  using Bejebeje.DataAccess.Context;
  using Bejebeje.Services.Extensions;
  using Bejebeje.Services.Services.Interfaces;
  using Bejebeje.ViewModels.Artist;
  using Microsoft.EntityFrameworkCore;

  public class ArtistsService : IArtistsService
  {
    private readonly BbContext context;

    public ArtistsService(BbContext context)
    {
      this.context = context;
    }

    public async Task<int> GetArtistIdAsync(string artistSlug)
    {
      int? artistId = await context
        .Artists
        .AsNoTracking()
        .Where(x => x.Slugs.Any(y => y.Name == artistSlug.Standardize()))
        .Select(x => (int?)x.Id)
        .FirstOrDefaultAsync();

      if (artistId == null)
      {
        throw new ArtistNotFoundException(artistSlug);
      }

      return artistId.Value;
    }

    public async Task<ArtistDetailsViewModel> GetArtistDetailsAsync(string artistSlug)
    {
      int artistId = await GetArtistIdAsync(artistSlug);

      ArtistDetailsViewModel artist = await context
        .Artists
        .AsNoTracking()
        .Where(x => x.Id == artistId)
        .Select(x => new ArtistDetailsViewModel
        {
          Id = x.Id,
          FirstName = x.FirstName,
          LastName = x.LastName,
          Slug = x.Slugs.Where(y => y.IsPrimary).First().Name,
          ImageId = x.Image != null ? x.Image.Id : 0,
          CreatedAt = x.CreatedAt,
          ModifiedAt = x.ModifiedAt
        })
        .SingleOrDefaultAsync();

      return artist;
    }

    public async Task<IList<ArtistCardViewModel>> GetArtistsAsync(int offset, int limit)
    {
      List<ArtistCardViewModel> artists = await context
        .Artists
        .AsNoTracking()
          .OrderBy(x => x.FirstName)
          .Paging(offset, limit)
          .Select(x => new ArtistCardViewModel
          {
            FirstName = x.FirstName,
            LastName = x.LastName,
            Slug = x.Slugs.Where(y => y.IsPrimary).First().Name,
            ImageId = x.Image == null ? 0 : x.Image.Id
          })
          .ToListAsync();

      return artists;
    }

    public async Task<IList<ArtistCardViewModel>> SearchArtistsAsync(string artistName)
    {
      string searchTermStandardized = artistName.Standardize();

      List<ArtistCardViewModel> matchedArtists = await context
        .Artists
        .AsNoTracking()
        .Where(x =>
            EF.Functions.Like(x.FullName.Standardize(), $"%{searchTermStandardized}%") ||
            x.Slugs.Any(s => EF.Functions.Like(s.Name.Standardize(), $"%{searchTermStandardized}%")))
          .OrderBy(x => x.FirstName)
          .Select(x => new ArtistCardViewModel
          {
            FirstName = x.FirstName,
            LastName = x.LastName,
            Slug = x.Slugs.Single(s => s.IsPrimary).Name,
            ImageId = x.Image.Id
          })
          .ToListAsync();

      return matchedArtists;
    }
  }
}
