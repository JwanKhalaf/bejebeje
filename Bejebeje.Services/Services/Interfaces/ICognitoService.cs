namespace Bejebeje.Services.Services.Interfaces;

using System.Threading.Tasks;

public interface ICognitoService
{
  Task<string> GetPreferredUsernameAsync(string userId);
}