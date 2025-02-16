using System.Globalization;

namespace Bejebeje.Services.Services
{
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Common.Enums;
  using Bejebeje.Common.Helpers;
  using Common.Exceptions;
  using DataAccess.Context;
  using Interfaces;
  using Microsoft.EntityFrameworkCore;
  using Models.Author;

  public class AuthorService : IAuthorService
  {
    private readonly BbContext _context;
    
    private readonly TextInfo _textInfo = new CultureInfo("ku-TR", false).TextInfo;

    public AuthorService(
      BbContext context)
    {
      _context = context;
    }

    public async Task<AuthorDetailsViewModel> GetAuthorDetailsAsync(
      string authorSlug)
    {
      AuthorDetailsViewModel authorDetails = await _context
        .Authors
        .AsNoTracking()
        .Include(a => a.Lyrics)
        .ThenInclude(x => x.Slugs)
        .Include(x => x.Lyrics)
        .ThenInclude(l => l.Artist)
        .ThenInclude(a => a.Slugs)
        .Where(a => a.Slugs.Any(s => s.Name == authorSlug))
        .Select(a => new AuthorDetailsViewModel
        {
          FirstName = _textInfo.ToTitleCase(a.FirstName),
          LastName = _textInfo.ToTitleCase(a.LastName),
          FullName = _textInfo.ToTitleCase(a.FullName),
          Biography = a.Biography,
          Slug = a.Slugs.Single(s => s.IsPrimary).Name,
          ImageUrl = ImageUrlBuilder.BuildImageUrl(a.HasImage, a.Id, ImageSize.Small),
          ImageAlternateText = ImageUrlBuilder.GetImageAlternateText(a.HasImage, a.FullName),
          CreatedAt = a.CreatedAt,
          ModifiedAt = a.ModifiedAt,
          Lyrics = a.Lyrics.Select(x => new AuthorLyricViewModel
          {
            Title = x.Title,
            ArtistSlug = x.Artist.Slugs.SingleOrDefault(slug => slug.IsPrimary && slug.IsDeleted == false).Name,
            LyricSlug = x.Slugs.SingleOrDefault(s => s.IsPrimary && s.IsDeleted == false).Name,
          }).ToList(),
        })
        .SingleOrDefaultAsync();

      if (authorDetails == null)
      {
        throw new AuthorNotFoundException(authorSlug);
      }

      return authorDetails;
    }
  }
}