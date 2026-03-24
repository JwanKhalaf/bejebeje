namespace Bejebeje.Services.Services.Interfaces;

using System.Threading.Tasks;
using Bejebeje.Domain;
using Bejebeje.Models.BbPoints;

public interface IBbPointsService
{
  Task<User> EnsureUserExistsAsync(string cognitoUserId, string username);

  Task<NavBarPointsViewModel> GetNavBarDataAsync(string cognitoUserId);

  Task<OwnProfileViewModel> GetOwnProfileDataAsync(string cognitoUserId);

  Task<PublicProfileViewModel> GetPublicProfileDataAsync(string username);

  Task<SubmitterPointsViewModel> GetSubmitterPointsAsync(string cognitoUserId);

  Task<bool> AwardSubmissionPointsAsync(string cognitoUserId, string username, PointActionType actionType, int entityId, string entityName, int points);

  Task AwardApprovalPointsAsync(string cognitoUserId, string username, PointActionType actionType, int entityId, string entityName, int points);

  Task<int> GetDailySubmissionCountAsync(string cognitoUserId, PointActionType actionType);
}
