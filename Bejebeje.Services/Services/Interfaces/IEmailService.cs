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

  Task SendLyricReportNotificationEmailAsync(
    string reporterUsername,
    string lyricTitle,
    string artistName,
    string categoryDisplayLabel,
    string comment);

  Task SendLyricReportConfirmationEmailAsync(
    string reporterEmail,
    string lyricTitle,
    string artistName);
}