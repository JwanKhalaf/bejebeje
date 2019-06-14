namespace Bejebeje.Services.Services
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Common.Extensions;
  using Bejebeje.DataAccess.Context;
  using Bejebeje.Models.Artist;
  using Bejebeje.Models.ArtistSlug;
  using Bejebeje.Models.Paging;
  using Bejebeje.Services.Extensions;
  using Bejebeje.Services.Services.Interfaces;
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

    public async Task<ArtistDetailsResponse> GetArtistDetailsAsync(string artistSlug)
    {
      int artistId = await GetArtistIdAsync(artistSlug);

      ArtistDetailsResponse artist = await context
        .Artists
        .AsNoTracking()
        .Where(x => x.Id == artistId)
        .Select(x => new ArtistDetailsResponse
        {
          Id = x.Id,
          FirstName = x.FirstName,
          LastName = x.LastName,
          Slug = x.Slugs.Where(y => y.IsPrimary).First().Name,
          ImageId = x.Image != null ? x.Image.Id : 0,
          CreatedAt = x.CreatedAt,
          ModifiedAt = x.ModifiedAt,
        })
        .SingleOrDefaultAsync();

      return artist;
    }

    public async Task<PagedArtistsResponse> GetArtistsAsync(int offset, int limit)
    {
      List<ArtistsResponse> artists = await context
        .Artists
        .AsNoTracking()
          .OrderBy(x => x.FirstName)
          .Paging(offset, limit)
          .Select(x => new ArtistsResponse
          {
            FirstName = x.FirstName,
            LastName = x.LastName,
            Slugs = x.Slugs.Select(s => new ArtistSlugResponse { Name = s.Name, IsPrimary = s.IsPrimary }).ToList(),
            ImageId = x.Image == null ? 0 : x.Image.Id,
          })
          .ToListAsync();

      PagedArtistsResponse response = new PagedArtistsResponse
      {
        Artists = artists,
        Paging = new PagingResponse
        {
          Offset = offset,
          Limit = limit,
        },
      };

      return response;
    }

    public async Task<ICollection<ArtistsResponse>> SearchArtistsAsync(string artistName)
    {
      if (string.IsNullOrEmpty(artistName))
      {
        return new List<ArtistsResponse>();
      }

      string searchTermStandardized = artistName.Standardize();

      List<ArtistsResponse> matchedArtists = await context
        .Artists
        .AsNoTracking()
        .Where(x =>
            EF.Functions.Like(x.FullName.Standardize(), $"%{searchTermStandardized}%") ||
            x.Slugs.Any(s => EF.Functions.Like(s.Name.Standardize(), $"%{searchTermStandardized}%")))
          .OrderBy(x => x.FirstName)
          .Select(x => new ArtistsResponse
          {
            FirstName = x.FirstName,
            LastName = x.LastName,
            Slugs = x.Slugs
              .Where(s => !s.IsDeleted)
              .Select(s => new ArtistSlugResponse { Name = s.Name, IsPrimary = s.IsPrimary })
              .ToList(),
            ImageId = x.Image.Id,
          })
          .ToListAsync();

      return matchedArtists;
    }
  }
}
