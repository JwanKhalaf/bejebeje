namespace Bejebeje.Services.Services.Interfaces;

using System.Threading.Tasks;
using Bejebeje.Models.Report;

public interface ILyricReportsService
{
  Task<int> GetReportCountForUserTodayAsync(string userId);

  Task<bool> HasPendingReportForLyricAsync(string userId, int lyricId);

  Task<LyricReportViewModel> GetLyricDetailsForReportAsync(string artistSlug, string lyricSlug);

  Task<int> CreateReportAsync(string userId, int lyricId, int category, string comment);

  Task SendReportEmailsAsync(
    string userId,
    string reporterEmail,
    string lyricTitle,
    string artistName,
    string categoryDisplayLabel,
    string comment);
}
