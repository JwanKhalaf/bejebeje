namespace Bejebeje.Services.Services
{
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Common.Extensions;
  using Bejebeje.DataAccess.Context;
  using Bejebeje.Domain;
  using Bejebeje.Models.Artist;
  using Bejebeje.Models.Paging;
  using Bejebeje.Services.Extensions;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.EntityFrameworkCore;
  using NodaTime;

  public class ArtistsService : IArtistsService
  {
    private readonly IArtistSlugsService artistSlugsService;

    private readonly BbContext context;

    private readonly TextInfo textInfo = new CultureInfo("ku-TR", false).TextInfo;

    public ArtistsService(
      IArtistSlugsService artistSlugsService,
      BbContext context)
    {
      this.artistSlugsService = artistSlugsService;
      this.context = context;
    }

    public async Task<int> GetArtistIdAsync(string artistSlug)
    {
      int? artistId = await context
        .Artists
        .AsNoTracking()
        .Where(x => x.Slugs.Any(y => y.Name == artistSlug.Standardize()) && x.IsApproved && !x.IsDeleted)
        .Select(x => (int?)x.Id)
        .FirstOrDefaultAsync();

      if (artistId == null)
      {
        throw new ArtistNotFoundException(artistSlug);
      }

      return artistId.Value;
    }

    public async Task<bool> ArtistExistsAsync(string artistSlug)
    {
      int? artistId = await context
        .Artists
        .AsNoTracking()
        .Where(x => x.Slugs.Any(y => y.Name == artistSlug.Standardize()))
        .Select(x => (int?)x.Id)
        .FirstOrDefaultAsync();

      if (artistId == null)
      {
        return false;
      }

      return true;
    }

    public async Task<GetArtistResponse> GetArtistDetailsAsync(string artistSlug)
    {
      int artistId = await GetArtistIdAsync(artistSlug);

      GetArtistResponse artist = await context
        .Artists
        .AsNoTracking()
        .Where(x => x.Id == artistId)
        .Select(x => new GetArtistResponse
        {
          Id = x.Id,
          FirstName = textInfo.ToTitleCase(x.FirstName),
          LastName = textInfo.ToTitleCase(x.LastName),
          FullName = textInfo.ToTitleCase(x.FullName),
          PrimarySlug = x.Slugs.Single(y => !y.IsDeleted && y.IsPrimary).Name,
          HasImage = x.Image != null,
          CreatedAt = x.CreatedAt,
          ModifiedAt = x.ModifiedAt,
        })
        .SingleOrDefaultAsync();

      return artist;
    }

    public async Task<CreateNewArtistResponse> CreateNewArtistAsync(CreateNewArtistRequest request)
    {
      string artistFullName = string.IsNullOrEmpty(request.LastName) ? request.FirstName : $"{request.FirstName} {request.LastName}";

      string artistSlug = artistSlugsService.GetArtistSlug(artistFullName);

      bool artistExists = await ArtistExistsAsync(artistSlug);

      if (artistExists)
      {
        throw new ArtistExistsException(artistSlug);
      }

      Artist artist = new Artist
      {
        FirstName = request.FirstName,
        LastName = request.LastName,
        FullName = artistFullName,
        Slugs = new List<ArtistSlug> { artistSlugsService.BuildArtistSlug(artistFullName) },
        CreatedAt = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc(),
      };

      context.Artists.Add(artist);
      await context.SaveChangesAsync();

      CreateNewArtistResponse response = new CreateNewArtistResponse
      {
        Slug = artistSlug,
        CreatedAt = artist.CreatedAt,
      };

      return response;
    }

    public async Task<PagedArtistSearchResponse> SearchArtistsAsync(string artistName, int offset, int limit)
    {
      int totalRecords;
      List<ArtistSearchResponse> artists;

      IOrderedQueryable<Artist> orderedArtists = context
        .Artists
        .AsNoTracking()
        .OrderBy(x => x.FirstName);

      if (!string.IsNullOrEmpty(artistName))
      {
        string searchTermStandardized = artistName.Standardize();

        totalRecords = await orderedArtists
          .Where(x => EF.Functions.Like(x.FullName.ToLower(), $"%{searchTermStandardized}%") || x.Slugs.Any(s => EF.Functions.Like(s.Name.ToLower(), $"%{searchTermStandardized}%")))
          .CountAsync();

        artists = await orderedArtists
          .Where(x => EF.Functions.Like(x.FullName.ToLower(), $"%{searchTermStandardized}%") || x.Slugs.Any(s => EF.Functions.Like(s.Name.ToLower(), $"%{searchTermStandardized}%")))
          .OrderBy(x => x.FirstName)
          .Select(x => new ArtistSearchResponse
          {
            FullName = textInfo.ToTitleCase(x.FullName),
            PrimarySlug = x.Slugs.Single(s => !s.IsDeleted && s.IsPrimary).Name,
            HasImage = x.Image != null,
          })
          .ToListAsync();
      }
      else
      {
        totalRecords = await orderedArtists.CountAsync();

        artists = await orderedArtists
          .Paging(offset, limit)
          .Select(x => new ArtistSearchResponse
          {
            FullName = textInfo.ToTitleCase(x.FullName),
            PrimarySlug = x.Slugs.Single(s => !s.IsDeleted && s.IsPrimary).Name,
            HasImage = x.Image != null,
          })
          .ToListAsync();
      }

      PagedArtistSearchResponse pagedArtistsResponse = new PagedArtistSearchResponse
      {
        Artists = artists,
        Paging = new PagingResponse
        {
          Offset = offset,
          Limit = limit,
          Total = totalRecords,
        },
      };

      return pagedArtistsResponse;
    }
  }
}
