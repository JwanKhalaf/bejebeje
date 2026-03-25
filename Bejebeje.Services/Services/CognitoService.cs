namespace Bejebeje.Services.Services;

using System;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Bejebeje.Services.Config;
using Bejebeje.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class CognitoService : ICognitoService
{
  private readonly IAmazonCognitoIdentityProvider _cognitoClient;
  private readonly string _userPoolId;
  private readonly ILogger<CognitoService> _logger;

  public CognitoService(
    IAmazonCognitoIdentityProvider cognitoClient,
    IOptions<CognitoOptions> options,
    ILogger<CognitoService> logger)
  {
    _cognitoClient = cognitoClient;
    _userPoolId = options.Value.UserPoolId;
    _logger = logger;
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

  public async Task<CognitoUserInfo> GetUserByEmailAsync(string email)
  {
    try
    {
      _logger.LogDebug("looking up cognito user by email {Email}", email);

      AdminGetUserRequest request = new AdminGetUserRequest
      {
        UserPoolId = _userPoolId,
        Username = email,
      };

      AdminGetUserResponse response = await _cognitoClient.AdminGetUserAsync(request);

      string sub = response.UserAttributes.Find(attr => attr.Name == "sub")?.Value;
      string preferredUsername = response.UserAttributes.Find(attr => attr.Name == "preferred_username")?.Value;

      _logger.LogDebug("cognito user found for {Email}: sub={Sub}", email, sub);

      return new CognitoUserInfo(sub, preferredUsername);
    }
    catch (UserNotFoundException)
    {
      _logger.LogDebug("cognito user not found for {Email}", email);
      return null;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "error looking up cognito user by email {Email}", email);
      throw;
    }
  }
}
