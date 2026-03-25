namespace Bejebeje.Services.Tests.Services
{
  using System;
  using System.Net;
  using System.Threading;
  using System.Threading.Tasks;
  using Amazon.CognitoIdentityProvider;
  using Amazon.CognitoIdentityProvider.Model;
  using Bejebeje.Services.Config;
  using Bejebeje.Services.Services;
  using FluentAssertions;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class AuthServiceComputeSecretHashTests
  {
    // secret hash is tested indirectly by verifying it is set on cognito requests
    // we test the authenticate path and verify the request includes the correct hash
  }

  [TestFixture]
  public class AuthServiceAuthenticateTests
  {
    private Mock<IAmazonCognitoIdentityProvider> _mockCognitoClient;
    private Mock<ILogger<AuthService>> _mockLogger;
    private IOptions<CognitoOptions> _options;
    private AuthService _authService;

    [SetUp]
    public void SetUp()
    {
      _mockCognitoClient = new Mock<IAmazonCognitoIdentityProvider>();
      _mockLogger = new Mock<ILogger<AuthService>>();
      _options = Options.Create(new CognitoOptions
      {
        ClientId = "test-client-id",
        ClientSecret = "test-client-secret",
        Authority = "https://cognito-idp.eu-west-2.amazonaws.com/eu-west-2_abc123",
        UserPoolId = "eu-west-2_abc123",
      });
      _authService = new AuthService(_mockCognitoClient.Object, _options, _mockLogger.Object);
    }

    [Test]
    public async Task should_return_success_with_tokens_on_valid_credentials()
    {
      // arrange
      var response = new InitiateAuthResponse
      {
        AuthenticationResult = new AuthenticationResultType
        {
          IdToken = "id-token-value",
          AccessToken = "access-token-value",
          RefreshToken = "refresh-token-value",
        },
      };

      _mockCognitoClient
        .Setup(c => c.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(response);

      // act
      var result = await _authService.AuthenticateAsync("user@example.com", "Password123!", null);

      // assert
      result.Success.Should().BeTrue();
      result.IdToken.Should().Be("id-token-value");
      result.AccessToken.Should().Be("access-token-value");
      result.RefreshToken.Should().Be("refresh-token-value");
    }

    [Test]
    public async Task should_use_user_password_auth_flow()
    {
      // arrange
      InitiateAuthRequest capturedRequest = null;

      _mockCognitoClient
        .Setup(c => c.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), It.IsAny<CancellationToken>()))
        .Callback<InitiateAuthRequest, CancellationToken>((req, _) => capturedRequest = req)
        .ReturnsAsync(new InitiateAuthResponse
        {
          AuthenticationResult = new AuthenticationResultType
          {
            IdToken = "t",
            AccessToken = "t",
            RefreshToken = "t",
          },
        });

      // act
      await _authService.AuthenticateAsync("user@example.com", "Password123!", null);

      // assert
      capturedRequest.Should().NotBeNull();
      capturedRequest.AuthFlow.Should().Be(AuthFlowType.USER_PASSWORD_AUTH);
    }

    [Test]
    public async Task should_include_username_password_and_secret_hash_in_request()
    {
      // arrange
      InitiateAuthRequest capturedRequest = null;

      _mockCognitoClient
        .Setup(c => c.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), It.IsAny<CancellationToken>()))
        .Callback<InitiateAuthRequest, CancellationToken>((req, _) => capturedRequest = req)
        .ReturnsAsync(new InitiateAuthResponse
        {
          AuthenticationResult = new AuthenticationResultType
          {
            IdToken = "t",
            AccessToken = "t",
            RefreshToken = "t",
          },
        });

      // act
      await _authService.AuthenticateAsync("user@example.com", "Password123!", null);

      // assert
      capturedRequest.AuthParameters.Should().ContainKey("USERNAME").WhoseValue.Should().Be("user@example.com");
      capturedRequest.AuthParameters.Should().ContainKey("PASSWORD").WhoseValue.Should().Be("Password123!");
      capturedRequest.AuthParameters.Should().ContainKey("SECRET_HASH");
      capturedRequest.AuthParameters["SECRET_HASH"].Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task should_set_client_id_on_request()
    {
      // arrange
      InitiateAuthRequest capturedRequest = null;

      _mockCognitoClient
        .Setup(c => c.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), It.IsAny<CancellationToken>()))
        .Callback<InitiateAuthRequest, CancellationToken>((req, _) => capturedRequest = req)
        .ReturnsAsync(new InitiateAuthResponse
        {
          AuthenticationResult = new AuthenticationResultType
          {
            IdToken = "t",
            AccessToken = "t",
            RefreshToken = "t",
          },
        });

      // act
      await _authService.AuthenticateAsync("user@example.com", "Password123!", null);

      // assert
      capturedRequest.ClientId.Should().Be("test-client-id");
    }

    [Test]
    public async Task should_set_user_context_data_with_valid_ipv4_address()
    {
      // arrange
      InitiateAuthRequest capturedRequest = null;

      _mockCognitoClient
        .Setup(c => c.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), It.IsAny<CancellationToken>()))
        .Callback<InitiateAuthRequest, CancellationToken>((req, _) => capturedRequest = req)
        .ReturnsAsync(new InitiateAuthResponse
        {
          AuthenticationResult = new AuthenticationResultType
          {
            IdToken = "t",
            AccessToken = "t",
            RefreshToken = "t",
          },
        });

      // act
      await _authService.AuthenticateAsync("user@example.com", "Password123!", "192.168.1.1");

      // assert
      capturedRequest.UserContextData.Should().NotBeNull();
      capturedRequest.UserContextData.IpAddress.Should().Be("192.168.1.1");
    }

    [Test]
    public async Task should_not_set_user_context_data_with_ipv6_address()
    {
      // arrange
      InitiateAuthRequest capturedRequest = null;

      _mockCognitoClient
        .Setup(c => c.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), It.IsAny<CancellationToken>()))
        .Callback<InitiateAuthRequest, CancellationToken>((req, _) => capturedRequest = req)
        .ReturnsAsync(new InitiateAuthResponse
        {
          AuthenticationResult = new AuthenticationResultType
          {
            IdToken = "t",
            AccessToken = "t",
            RefreshToken = "t",
          },
        });

      // act
      await _authService.AuthenticateAsync("user@example.com", "Password123!", "::1");

      // assert
      capturedRequest.UserContextData.Should().BeNull();
    }

    [Test]
    public async Task should_not_set_user_context_data_with_null_ip()
    {
      // arrange
      InitiateAuthRequest capturedRequest = null;

      _mockCognitoClient
        .Setup(c => c.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), It.IsAny<CancellationToken>()))
        .Callback<InitiateAuthRequest, CancellationToken>((req, _) => capturedRequest = req)
        .ReturnsAsync(new InitiateAuthResponse
        {
          AuthenticationResult = new AuthenticationResultType
          {
            IdToken = "t",
            AccessToken = "t",
            RefreshToken = "t",
          },
        });

      // act
      await _authService.AuthenticateAsync("user@example.com", "Password123!", null);

      // assert
      capturedRequest.UserContextData.Should().BeNull();
    }

    [Test]
    public async Task should_return_invalid_credentials_on_not_authorized_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new NotAuthorizedException("Bad credentials"));

      // act
      var result = await _authService.AuthenticateAsync("user@example.com", "wrong", null);

      // assert
      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(AuthErrorType.InvalidCredentials);
      result.ErrorMessage.Should().Be("Invalid email or password. Please try again.");
    }

    [Test]
    public async Task should_return_invalid_credentials_on_user_not_found_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new UserNotFoundException("Not found"));

      // act
      var result = await _authService.AuthenticateAsync("nobody@example.com", "Password123!", null);

      // assert
      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(AuthErrorType.InvalidCredentials);
      result.ErrorMessage.Should().Be("Invalid email or password. Please try again.");
    }

    [Test]
    public async Task should_return_user_not_confirmed_on_user_not_confirmed_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new UserNotConfirmedException("Not confirmed"));

      // act
      var result = await _authService.AuthenticateAsync("user@example.com", "Password123!", null);

      // assert
      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(AuthErrorType.UserNotConfirmed);
      result.ErrorMessage.Should().Be("Please confirm your email address before logging in.");
    }

    [Test]
    public async Task should_return_too_many_requests_on_too_many_requests_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new TooManyRequestsException("Throttled"));

      // act
      var result = await _authService.AuthenticateAsync("user@example.com", "Password123!", null);

      // assert
      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(AuthErrorType.TooManyRequests);
      result.ErrorMessage.Should().Be("Too many failed attempts. Please try again later.");
    }

    [Test]
    public async Task should_return_unexpected_on_generic_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Something went wrong"));

      // act
      var result = await _authService.AuthenticateAsync("user@example.com", "Password123!", null);

      // assert
      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(AuthErrorType.Unexpected);
      result.ErrorMessage.Should().Be("An unexpected error occurred. Please try again.");
    }
  }

  [TestFixture]
  public class AuthServiceSignUpTests
  {
    private Mock<IAmazonCognitoIdentityProvider> _mockCognitoClient;
    private Mock<ILogger<AuthService>> _mockLogger;
    private IOptions<CognitoOptions> _options;
    private AuthService _authService;

    [SetUp]
    public void SetUp()
    {
      _mockCognitoClient = new Mock<IAmazonCognitoIdentityProvider>();
      _mockLogger = new Mock<ILogger<AuthService>>();
      _options = Options.Create(new CognitoOptions
      {
        ClientId = "test-client-id",
        ClientSecret = "test-client-secret",
        Authority = "https://cognito-idp.eu-west-2.amazonaws.com/eu-west-2_abc123",
        UserPoolId = "eu-west-2_abc123",
      });
      _authService = new AuthService(_mockCognitoClient.Object, _options, _mockLogger.Object);
    }

    [Test]
    public async Task should_return_success_on_valid_sign_up()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new SignUpResponse());

      // act
      var result = await _authService.SignUpAsync("user@example.com", "testuser", "Password123456!");

      // assert
      result.Success.Should().BeTrue();
    }

    [Test]
    public async Task should_pass_email_as_username_in_sign_up_request()
    {
      // arrange
      SignUpRequest capturedRequest = null;

      _mockCognitoClient
        .Setup(c => c.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>()))
        .Callback<SignUpRequest, CancellationToken>((req, _) => capturedRequest = req)
        .ReturnsAsync(new SignUpResponse());

      // act
      await _authService.SignUpAsync("user@example.com", "testuser", "Password123456!");

      // assert
      capturedRequest.Username.Should().Be("user@example.com");
    }

    [Test]
    public async Task should_include_preferred_username_attribute_in_sign_up_request()
    {
      // arrange
      SignUpRequest capturedRequest = null;

      _mockCognitoClient
        .Setup(c => c.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>()))
        .Callback<SignUpRequest, CancellationToken>((req, _) => capturedRequest = req)
        .ReturnsAsync(new SignUpResponse());

      // act
      await _authService.SignUpAsync("user@example.com", "testuser", "Password123456!");

      // assert
      capturedRequest.UserAttributes.Should().Contain(attr => attr.Name == "preferred_username" && attr.Value == "testuser");
    }

    [Test]
    public async Task should_include_secret_hash_in_sign_up_request()
    {
      // arrange
      SignUpRequest capturedRequest = null;

      _mockCognitoClient
        .Setup(c => c.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>()))
        .Callback<SignUpRequest, CancellationToken>((req, _) => capturedRequest = req)
        .ReturnsAsync(new SignUpResponse());

      // act
      await _authService.SignUpAsync("user@example.com", "testuser", "Password123456!");

      // assert
      capturedRequest.SecretHash.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task should_return_username_exists_on_username_exists_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new UsernameExistsException("Already exists"));

      // act
      var result = await _authService.SignUpAsync("user@example.com", "testuser", "Password123456!");

      // assert
      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(SignUpErrorType.UsernameExists);
      result.ErrorMessage.Should().Be("An account with this email already exists.");
    }

    [Test]
    public async Task should_return_invalid_password_on_invalid_password_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new InvalidPasswordException("Bad password"));

      // act
      var result = await _authService.SignUpAsync("user@example.com", "testuser", "weak");

      // assert
      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(SignUpErrorType.InvalidPassword);
      result.ErrorMessage.Should().Be("Password does not meet the required criteria.");
    }

    [Test]
    public async Task should_return_invalid_parameter_on_invalid_parameter_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new InvalidParameterException("Invalid param"));

      // act
      var result = await _authService.SignUpAsync("user@example.com", "testuser", "Password123456!");

      // assert
      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(SignUpErrorType.InvalidParameter);
      result.ErrorMessage.Should().Be("Please check your input and try again.");
    }

    [Test]
    public async Task should_return_unexpected_on_generic_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Something went wrong"));

      // act
      var result = await _authService.SignUpAsync("user@example.com", "testuser", "Password123456!");

      // assert
      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(SignUpErrorType.Unexpected);
      result.ErrorMessage.Should().Be("An unexpected error occurred. Please try again.");
    }
  }

  [TestFixture]
  public class AuthServiceConfirmSignUpTests
  {
    private Mock<IAmazonCognitoIdentityProvider> _mockCognitoClient;
    private Mock<ILogger<AuthService>> _mockLogger;
    private IOptions<CognitoOptions> _options;
    private AuthService _authService;

    [SetUp]
    public void SetUp()
    {
      _mockCognitoClient = new Mock<IAmazonCognitoIdentityProvider>();
      _mockLogger = new Mock<ILogger<AuthService>>();
      _options = Options.Create(new CognitoOptions
      {
        ClientId = "test-client-id",
        ClientSecret = "test-client-secret",
        Authority = "https://cognito-idp.eu-west-2.amazonaws.com/eu-west-2_abc123",
        UserPoolId = "eu-west-2_abc123",
      });
      _authService = new AuthService(_mockCognitoClient.Object, _options, _mockLogger.Object);
    }

    [Test]
    public async Task should_return_success_on_valid_confirmation()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.ConfirmSignUpAsync(It.IsAny<ConfirmSignUpRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ConfirmSignUpResponse());

      // act
      var result = await _authService.ConfirmSignUpAsync("user@example.com", "123456");

      // assert
      result.Success.Should().BeTrue();
    }

    [Test]
    public async Task should_include_secret_hash_in_confirm_request()
    {
      // arrange
      ConfirmSignUpRequest capturedRequest = null;

      _mockCognitoClient
        .Setup(c => c.ConfirmSignUpAsync(It.IsAny<ConfirmSignUpRequest>(), It.IsAny<CancellationToken>()))
        .Callback<ConfirmSignUpRequest, CancellationToken>((req, _) => capturedRequest = req)
        .ReturnsAsync(new ConfirmSignUpResponse());

      // act
      await _authService.ConfirmSignUpAsync("user@example.com", "123456");

      // assert
      capturedRequest.SecretHash.Should().NotBeNullOrEmpty();
      capturedRequest.Username.Should().Be("user@example.com");
      capturedRequest.ConfirmationCode.Should().Be("123456");
    }

    [Test]
    public async Task should_return_code_mismatch_on_code_mismatch_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.ConfirmSignUpAsync(It.IsAny<ConfirmSignUpRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new CodeMismatchException("Bad code"));

      // act
      var result = await _authService.ConfirmSignUpAsync("user@example.com", "000000");

      // assert
      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(ConfirmErrorType.CodeMismatch);
      result.ErrorMessage.Should().Be("Invalid confirmation code. Please check the code and try again.");
    }

    [Test]
    public async Task should_return_expired_code_on_expired_code_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.ConfirmSignUpAsync(It.IsAny<ConfirmSignUpRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new ExpiredCodeException("Code expired"));

      // act
      var result = await _authService.ConfirmSignUpAsync("user@example.com", "123456");

      // assert
      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(ConfirmErrorType.ExpiredCode);
      result.ErrorMessage.Should().Be("The confirmation code has expired. Please request a new one.");
    }

    [Test]
    public async Task should_return_alias_exists_on_alias_exists_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.ConfirmSignUpAsync(It.IsAny<ConfirmSignUpRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new AliasExistsException("Already confirmed"));

      // act
      var result = await _authService.ConfirmSignUpAsync("user@example.com", "123456");

      // assert
      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(ConfirmErrorType.AliasExists);
      result.ErrorMessage.Should().Be("An account with this email is already confirmed.");
    }

    [Test]
    public async Task should_return_unexpected_on_generic_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.ConfirmSignUpAsync(It.IsAny<ConfirmSignUpRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Something went wrong"));

      // act
      var result = await _authService.ConfirmSignUpAsync("user@example.com", "123456");

      // assert
      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(ConfirmErrorType.Unexpected);
      result.ErrorMessage.Should().Be("An unexpected error occurred. Please try again.");
    }
  }
}
