namespace Bejebeje.Services.Services.Interfaces
{
  using System.Threading.Tasks;
  using Models.Author;

  public interface IAuthorService
  {
    Task<AuthorDetailsViewModel> GetAuthorDetailsAsync(string authorSlug);
  }
}
