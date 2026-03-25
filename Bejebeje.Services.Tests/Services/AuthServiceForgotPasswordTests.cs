namespace Bejebeje.Services.Tests.Services
{
  using System;
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
  public class AuthServiceForgotPasswordTests
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
    public async Task should_return_success_on_valid_forgot_password()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.ForgotPasswordAsync(It.IsAny<ForgotPasswordRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ForgotPasswordResponse());

      // act
      var result = await _authService.ForgotPasswordAsync("user@example.com");

      // assert
      result.Success.Should().BeTrue();
    }

    [Test]
    public async Task should_include_secret_hash_in_forgot_password_request()
    {
      // arrange
      ForgotPasswordRequest capturedRequest = null;

      _mockCognitoClient
        .Setup(c => c.ForgotPasswordAsync(It.IsAny<ForgotPasswordRequest>(), It.IsAny<CancellationToken>()))
        .Callback<ForgotPasswordRequest, CancellationToken>((req, _) => capturedRequest = req)
        .ReturnsAsync(new ForgotPasswordResponse());

      // act
      await _authService.ForgotPasswordAsync("user@example.com");

      // assert
      capturedRequest.SecretHash.Should().NotBeNullOrEmpty();
      capturedRequest.Username.Should().Be("user@example.com");
    }

    [Test]
    public async Task should_treat_user_not_found_as_success_for_security()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.ForgotPasswordAsync(It.IsAny<ForgotPasswordRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new UserNotFoundException("Not found"));

      // act
      var result = await _authService.ForgotPasswordAsync("nobody@example.com");

      // assert
      result.Success.Should().BeTrue();
    }

    [Test]
    public async Task should_return_failure_on_generic_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.ForgotPasswordAsync(It.IsAny<ForgotPasswordRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Something went wrong"));

      // act
      var result = await _authService.ForgotPasswordAsync("user@example.com");

      // assert
      result.Success.Should().BeFalse();
      result.ErrorMessage.Should().Be("An unexpected error occurred. Please try again.");
    }
  }

  [TestFixture]
  public class AuthServiceConfirmForgotPasswordTests
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
    public async Task should_return_success_on_valid_reset()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.ConfirmForgotPasswordAsync(It.IsAny<ConfirmForgotPasswordRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ConfirmForgotPasswordResponse());

      // act
      var result = await _authService.ConfirmForgotPasswordAsync("user@example.com", "123456", "NewPassword123!");

      // assert
      result.Success.Should().BeTrue();
    }

    [Test]
    public async Task should_include_secret_hash_in_confirm_forgot_password_request()
    {
      // arrange
      ConfirmForgotPasswordRequest capturedRequest = null;

      _mockCognitoClient
        .Setup(c => c.ConfirmForgotPasswordAsync(It.IsAny<ConfirmForgotPasswordRequest>(), It.IsAny<CancellationToken>()))
        .Callback<ConfirmForgotPasswordRequest, CancellationToken>((req, _) => capturedRequest = req)
        .ReturnsAsync(new ConfirmForgotPasswordResponse());

      // act
      await _authService.ConfirmForgotPasswordAsync("user@example.com", "123456", "NewPassword123!");

      // assert
      capturedRequest.SecretHash.Should().NotBeNullOrEmpty();
      capturedRequest.Username.Should().Be("user@example.com");
      capturedRequest.ConfirmationCode.Should().Be("123456");
      capturedRequest.Password.Should().Be("NewPassword123!");
    }

    [Test]
    public async Task should_return_code_mismatch_on_code_mismatch_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.ConfirmForgotPasswordAsync(It.IsAny<ConfirmForgotPasswordRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new CodeMismatchException("Bad code"));

      // act
      var result = await _authService.ConfirmForgotPasswordAsync("user@example.com", "000000", "NewPassword123!");

      // assert
      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(ResetErrorType.CodeMismatch);
      result.ErrorMessage.Should().Be("Invalid confirmation code. Please check the code and try again.");
    }

    [Test]
    public async Task should_return_expired_code_on_expired_code_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.ConfirmForgotPasswordAsync(It.IsAny<ConfirmForgotPasswordRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new ExpiredCodeException("Expired"));

      // act
      var result = await _authService.ConfirmForgotPasswordAsync("user@example.com", "123456", "NewPassword123!");

      // assert
      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(ResetErrorType.ExpiredCode);
    }

    [Test]
    public async Task should_return_invalid_password_on_invalid_password_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.ConfirmForgotPasswordAsync(It.IsAny<ConfirmForgotPasswordRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new InvalidPasswordException("Bad password"));

      // act
      var result = await _authService.ConfirmForgotPasswordAsync("user@example.com", "123456", "weak");

      // assert
      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(ResetErrorType.InvalidPassword);
    }

    [Test]
    public async Task should_return_user_not_found_on_user_not_found_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.ConfirmForgotPasswordAsync(It.IsAny<ConfirmForgotPasswordRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new UserNotFoundException("Not found"));

      // act
      var result = await _authService.ConfirmForgotPasswordAsync("nobody@example.com", "123456", "NewPassword123!");

      // assert
      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(ResetErrorType.UserNotFound);
      result.ErrorMessage.Should().Be("Unable to reset password. Please try again.");
    }

    [Test]
    public async Task should_return_unexpected_on_generic_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.ConfirmForgotPasswordAsync(It.IsAny<ConfirmForgotPasswordRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Something went wrong"));

      // act
      var result = await _authService.ConfirmForgotPasswordAsync("user@example.com", "123456", "NewPassword123!");

      // assert
      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(ResetErrorType.Unexpected);
      result.ErrorMessage.Should().Be("Unable to reset password. Please try again.");
    }
  }

  [TestFixture]
  public class AuthServiceResendConfirmationCodeTests
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
    public async Task should_return_success_on_valid_resend()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.ResendConfirmationCodeAsync(It.IsAny<ResendConfirmationCodeRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ResendConfirmationCodeResponse());

      // act
      var result = await _authService.ResendConfirmationCodeAsync("user@example.com");

      // assert
      result.Success.Should().BeTrue();
    }

    [Test]
    public async Task should_include_secret_hash_in_resend_request()
    {
      // arrange
      ResendConfirmationCodeRequest capturedRequest = null;

      _mockCognitoClient
        .Setup(c => c.ResendConfirmationCodeAsync(It.IsAny<ResendConfirmationCodeRequest>(), It.IsAny<CancellationToken>()))
        .Callback<ResendConfirmationCodeRequest, CancellationToken>((req, _) => capturedRequest = req)
        .ReturnsAsync(new ResendConfirmationCodeResponse());

      // act
      await _authService.ResendConfirmationCodeAsync("user@example.com");

      // assert
      capturedRequest.SecretHash.Should().NotBeNullOrEmpty();
      capturedRequest.Username.Should().Be("user@example.com");
    }

    [Test]
    public async Task should_return_failure_on_generic_exception()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.ResendConfirmationCodeAsync(It.IsAny<ResendConfirmationCodeRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Something went wrong"));

      // act
      var result = await _authService.ResendConfirmationCodeAsync("user@example.com");

      // assert
      result.Success.Should().BeFalse();
      result.ErrorMessage.Should().Be("An unexpected error occurred. Please try again.");
    }
  }
}
