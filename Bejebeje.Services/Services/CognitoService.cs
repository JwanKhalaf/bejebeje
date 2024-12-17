namespace Bejebeje.Services.Services;

using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Bejebeje.Services.Services.Interfaces;
using Microsoft.Extensions.Configuration;

public class CognitoService : ICognitoService
{
  private readonly IAmazonCognitoIdentityProvider _cognitoClient;
  private readonly string _userPoolId;

  public CognitoService(
    IAmazonCognitoIdentityProvider cognitoClient,
    IConfiguration configuration)
  {
    _cognitoClient = cognitoClient;
    _userPoolId = configuration["Cognito:UserPoolId"];
  }

  public async Task<string> GetPreferredUsernameAsync(string userId)
  {
    try
    {
      AdminGetUserRequest request = new AdminGetUserRequest
      {
        UserPoolId = _userPoolId,
        Username = userId,
      };

      AdminGetUserResponse response = await _cognitoClient.AdminGetUserAsync(request);

      AttributeType preferredUsernameAttribute = response.UserAttributes
        .Find(attr => attr.Name == "preferred_username");

      return preferredUsernameAttribute?.Value ?? "Unknown User";
    }
    catch (UserNotFoundException)
    {
      return "Unknown User";
    }
  }
}