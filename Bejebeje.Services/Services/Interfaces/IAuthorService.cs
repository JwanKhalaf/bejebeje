namespace Bejebeje.Services.Services.Interfaces
{
  using System.Threading.Tasks;
  using Models.Author;

  public interface IAuthorService
  {
    Task<AuthorDetailsResponse> GetAuthorDetailsAsync(string authorSlug);
  }
}
