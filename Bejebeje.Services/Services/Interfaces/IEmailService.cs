namespace Bejebeje.Services.Services.Interfaces;

using System.Threading.Tasks;

public interface IEmailService
{
  Task SendArtistSubmissionEmailAsync(
    string username,
    string artistFullName);

  Task SendLyricSubmissionEmailAsync(
    string username,
    string lyricTitle,
    int lyricId,
    string artistFullName,
    int artistId);
}