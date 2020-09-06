namespace Bejebeje.Services.Services
{
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.DataAccess.Context;
  using Bejebeje.Models.Author;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.EntityFrameworkCore;

  public class AuthorService : IAuthorService
  {
    private readonly BbContext context;

    public AuthorService(
      BbContext context)
    {
      this.context = context;
    }

    public async Task<AuthorDetailsResponse> GetAuthorDetailsAsync(
      string authorSlug)
    {
      var temp = context
        .Authors
        .Where(a => a.Slugs.Any(s => s.Name == authorSlug)).FirstOrDefault();

      AuthorDetailsResponse authorDetails = await context
        .Authors
        .Where(a => a.Slugs.Any(s => s.Name == authorSlug))
        .Select(a => new AuthorDetailsResponse
        {
          FirstName = a.FirstName,
          LastName = a.LastName,
          Biography = a.Biography,
          Slug = a.Slugs.Where(s => s.IsPrimary).Single().Name,
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
