namespace Bejebeje.Services.Services.Interfaces;

using System.Threading.Tasks;

public interface IEmailService
{
  Task SendArtistSubmissionEmailAsync(string username, string artistFullName);
}