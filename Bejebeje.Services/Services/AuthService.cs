namespace Bejebeje.Services.Services;

using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Bejebeje.Services.Config;
using Bejebeje.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class AuthService : IAuthService
{
  private readonly IAmazonCognitoIdentityProvider _cognitoClient;
  private readonly CognitoOptions _options;
  private readonly ILogger<AuthService> _logger;

  public AuthService(
    IAmazonCognitoIdentityProvider cognitoClient,
    IOptions<CognitoOptions> options,
    ILogger<AuthService> logger)
  {
    _cognitoClient = cognitoClient;
    _options = options.Value;
    _logger = logger;
  }

  public async Task<AuthResult> AuthenticateAsync(string email, string password, string clientIp)
  {
    try
    {
      _logger.LogDebug("attempting authentication for {Email}", email);

      var request = new InitiateAuthRequest
      {
        AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
        ClientId = _options.ClientId,
        AuthParameters = new Dictionary<string, string>
        {
          { "USERNAME", email },
          { "PASSWORD", password },
          { "SECRET_HASH", ComputeSecretHash(email) },
        },
      };

      if (IsValidIpv4(clientIp))
      {
        request.UserContextData = new UserContextDataType
        {
          IpAddress = clientIp,
        };
      }

      var response = await _cognitoClient.InitiateAuthAsync(request);

      _logger.LogInformation("authentication successful for {Email}", email);

      return AuthResult.Succeed(
        response.AuthenticationResult.IdToken,
        response.AuthenticationResult.AccessToken,
        response.AuthenticationResult.RefreshToken);
    }
    catch (NotAuthorizedException)
    {
      _logger.LogWarning("authentication failed for {Email}: invalid credentials", email);
      return AuthResult.Fail(AuthErrorType.InvalidCredentials, "Invalid email or password. Please try again.");
    }
    catch (UserNotFoundException)
    {
      _logger.LogWarning("authentication failed for {Email}: user not found", email);
      return AuthResult.Fail(AuthErrorType.InvalidCredentials, "Invalid email or password. Please try again.");
    }
    catch (UserNotConfirmedException)
    {
      _logger.LogWarning("authentication failed for {Email}: user not confirmed", email);
      return AuthResult.Fail(AuthErrorType.UserNotConfirmed, "Please confirm your email address before logging in.");
    }
    catch (TooManyRequestsException)
    {
      _logger.LogWarning("authentication failed for {Email}: too many requests", email);
      return AuthResult.Fail(AuthErrorType.TooManyRequests, "Too many failed attempts. Please try again later.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "unexpected error during authentication for {Email}", email);
      return AuthResult.Fail(AuthErrorType.Unexpected, "An unexpected error occurred. Please try again.");
    }
  }

  public async Task<SignUpResult> SignUpAsync(string email, string username, string password)
  {
    try
    {
      _logger.LogDebug("attempting sign-up for {Email}", email);

      var request = new SignUpRequest
      {
        ClientId = _options.ClientId,
        Username = email,
        Password = password,
        SecretHash = ComputeSecretHash(email),
        UserAttributes = new List<AttributeType>
        {
          new AttributeType { Name = "preferred_username", Value = username },
        },
      };

      await _cognitoClient.SignUpAsync(request);

      _logger.LogInformation("sign-up successful for {Email}", email);

      return SignUpResult.Succeed();
    }
    catch (UsernameExistsException)
    {
      _logger.LogWarning("sign-up failed for {Email}: username already exists", email);
      return SignUpResult.Fail(SignUpErrorType.UsernameExists, "An account with this email already exists.");
    }
    catch (InvalidPasswordException)
    {
      _logger.LogWarning("sign-up failed for {Email}: invalid password", email);
      return SignUpResult.Fail(SignUpErrorType.InvalidPassword, "Password does not meet the required criteria.");
    }
    catch (InvalidParameterException)
    {
      _logger.LogWarning("sign-up failed for {Email}: invalid parameter", email);
      return SignUpResult.Fail(SignUpErrorType.InvalidParameter, "Please check your input and try again.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "unexpected error during sign-up for {Email}", email);
      return SignUpResult.Fail(SignUpErrorType.Unexpected, "An unexpected error occurred. Please try again.");
    }
  }

  public async Task<ConfirmResult> ConfirmSignUpAsync(string email, string code)
  {
    try
    {
      _logger.LogDebug("attempting email confirmation for {Email}", email);

      var request = new ConfirmSignUpRequest
      {
        ClientId = _options.ClientId,
        Username = email,
        ConfirmationCode = code,
        SecretHash = ComputeSecretHash(email),
      };

      await _cognitoClient.ConfirmSignUpAsync(request);

      _logger.LogInformation("email confirmation successful for {Email}", email);

      return ConfirmResult.Succeed();
    }
    catch (CodeMismatchException)
    {
      _logger.LogWarning("confirmation failed for {Email}: code mismatch", email);
      return ConfirmResult.Fail(ConfirmErrorType.CodeMismatch, "Invalid confirmation code. Please check the code and try again.");
    }
    catch (ExpiredCodeException)
    {
      _logger.LogWarning("confirmation failed for {Email}: code expired", email);
      return ConfirmResult.Fail(ConfirmErrorType.ExpiredCode, "The confirmation code has expired. Please request a new one.");
    }
    catch (AliasExistsException)
    {
      _logger.LogWarning("confirmation failed for {Email}: alias already exists", email);
      return ConfirmResult.Fail(ConfirmErrorType.AliasExists, "An account with this email is already confirmed.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "unexpected error during confirmation for {Email}", email);
      return ConfirmResult.Fail(ConfirmErrorType.Unexpected, "An unexpected error occurred. Please try again.");
    }
  }

  public async Task<ResendResult> ResendConfirmationCodeAsync(string email)
  {
    try
    {
      _logger.LogDebug("resending confirmation code for {Email}", email);

      var request = new ResendConfirmationCodeRequest
      {
        ClientId = _options.ClientId,
        Username = email,
        SecretHash = ComputeSecretHash(email),
      };

      await _cognitoClient.ResendConfirmationCodeAsync(request);

      _logger.LogInformation("confirmation code resent for {Email}", email);

      return ResendResult.Succeed();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "unexpected error resending confirmation code for {Email}", email);
      return ResendResult.Fail("An unexpected error occurred. Please try again.");
    }
  }

  public async Task<ForgotPasswordResult> ForgotPasswordAsync(string email)
  {
    try
    {
      _logger.LogDebug("initiating forgot password for {Email}", email);

      var request = new ForgotPasswordRequest
      {
        ClientId = _options.ClientId,
        Username = email,
        SecretHash = ComputeSecretHash(email),
      };

      await _cognitoClient.ForgotPasswordAsync(request);

      _logger.LogInformation("forgot password request successful for {Email}", email);

      return ForgotPasswordResult.Succeed();
    }
    catch (UserNotFoundException)
    {
      // treat as success for security (no information disclosure)
      _logger.LogDebug("forgot password for {Email}: user not found, treating as success", email);
      return ForgotPasswordResult.Succeed();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "unexpected error during forgot password for {Email}", email);
      return ForgotPasswordResult.Fail("An unexpected error occurred. Please try again.");
    }
  }

  public async Task<ResetPasswordResult> ConfirmForgotPasswordAsync(string email, string code, string newPassword)
  {
    try
    {
      _logger.LogDebug("attempting password reset for {Email}", email);

      var request = new ConfirmForgotPasswordRequest
      {
        ClientId = _options.ClientId,
        Username = email,
        ConfirmationCode = code,
        Password = newPassword,
        SecretHash = ComputeSecretHash(email),
      };

      await _cognitoClient.ConfirmForgotPasswordAsync(request);

      _logger.LogInformation("password reset successful for {Email}", email);

      return ResetPasswordResult.Succeed();
    }
    catch (CodeMismatchException)
    {
      _logger.LogWarning("password reset failed for {Email}: code mismatch", email);
      return ResetPasswordResult.Fail(ResetErrorType.CodeMismatch, "Invalid confirmation code. Please check the code and try again.");
    }
    catch (ExpiredCodeException)
    {
      _logger.LogWarning("password reset failed for {Email}: code expired", email);
      return ResetPasswordResult.Fail(ResetErrorType.ExpiredCode, "The confirmation code has expired. Please request a new one.");
    }
    catch (InvalidPasswordException)
    {
      _logger.LogWarning("password reset failed for {Email}: invalid password", email);
      return ResetPasswordResult.Fail(ResetErrorType.InvalidPassword, "Password does not meet the required criteria.");
    }
    catch (UserNotFoundException)
    {
      _logger.LogWarning("password reset failed for {Email}: user not found", email);
      return ResetPasswordResult.Fail(ResetErrorType.UserNotFound, "Unable to reset password. Please try again.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "unexpected error during password reset for {Email}", email);
      return ResetPasswordResult.Fail(ResetErrorType.Unexpected, "Unable to reset password. Please try again.");
    }
  }

  private string ComputeSecretHash(string username)
  {
    byte[] key = Encoding.UTF8.GetBytes(_options.ClientSecret);
    byte[] message = Encoding.UTF8.GetBytes(username + _options.ClientId);

    using var hmac = new HMACSHA256(key);
    byte[] hash = hmac.ComputeHash(message);

    return Convert.ToBase64String(hash);
  }

  private static bool IsValidIpv4(string ip)
  {
    if (string.IsNullOrEmpty(ip))
    {
      return false;
    }

    return IPAddress.TryParse(ip, out var address) && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
  }
}
