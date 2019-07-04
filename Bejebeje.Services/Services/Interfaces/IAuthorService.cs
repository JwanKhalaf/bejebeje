namespace Bejebeje.Services.Services.Interfaces
{
  using System.Threading.Tasks;
  using Bejebeje.Models.Author;

  public interface IAuthorService
  {
    Task<AuthorDetailsResponse> GetAuthorDetailsAsync(string authorSlug);
  }
}
