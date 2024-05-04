namespace Bejebeje.Services.Services.Interfaces;

using System.Threading.Tasks;
using Models.Artist;

public interface IEmailNotificationsService
{
  Task AcknowledgeArtistSubmission(
    string username,
    CreateArtistViewModel model);
}