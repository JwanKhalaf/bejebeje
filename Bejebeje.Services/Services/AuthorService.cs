namespace Bejebeje.Services.Services
{
  using System.Linq;
  using System.Threading.Tasks;
  using Common.Exceptions;
  using DataAccess.Context;
  using Interfaces;
  using Microsoft.EntityFrameworkCore;
  using Models.Author;

  public class AuthorService : IAuthorService
  {
    private readonly BbContext _context;

    public AuthorService(
      BbContext context)
    {
      _context = context;
    }

    public async Task<AuthorDetailsResponse> GetAuthorDetailsAsync(
      string authorSlug)
    {
      AuthorDetailsResponse authorDetails = await _context
        .Authors
        .Where(a => a.Slugs.Any(s => s.Name == authorSlug))
        .Select(a => new AuthorDetailsResponse
        {
          FirstName = a.FirstName,
          LastName = a.LastName,
          Biography = a.Biography,
          Slug = a.Slugs.Single(s => s.IsPrimary).Name,
          ImageId = a.Image.Id,
          CreatedAt = a.CreatedAt,
          ModifiedAt = a.ModifiedAt,
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
