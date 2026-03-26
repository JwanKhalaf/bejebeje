namespace Bejebeje.Services.Services.Interfaces
{
  using System.Threading.Tasks;
  using Bejebeje.Models.Homepage;

  public interface IHomepageService
  {
    Task<HomepageViewModel> GetHomepageViewModelAsync(bool isAuthenticated);
  }
}
