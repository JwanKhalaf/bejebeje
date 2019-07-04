namespace Bejebeje.Services.Services
{
  using System.Threading.Tasks;
  using Bejebeje.Models.Author;
  using Bejebeje.Services.Services.Interfaces;

  public class AuthorService : IAuthorService
  {
    public Task<AuthorDetailsResponse> GetAuthorDetailsAsync(string authorSlug)
    {
      throw new System.NotImplementedException();
    }
  }
}
