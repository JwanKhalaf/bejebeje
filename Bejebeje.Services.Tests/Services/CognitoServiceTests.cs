namespace Bejebeje.Services.Tests.Services
{
  using System;
  using System.Collections.Generic;
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
  public class CognitoServiceGetUserByEmailTests
  {
    private Mock<IAmazonCognitoIdentityProvider> _mockCognitoClient;
    private Mock<ILogger<CognitoService>> _mockLogger;
    private IOptions<CognitoOptions> _options;
    private CognitoService _cognitoService;

    [SetUp]
    public void SetUp()
    {
      _mockCognitoClient = new Mock<IAmazonCognitoIdentityProvider>();
      _mockLogger = new Mock<ILogger<CognitoService>>();
      _options = Options.Create(new CognitoOptions
      {
        ClientId = "test-client-id",
        ClientSecret = "test-client-secret",
        Authority = "https://cognito-idp.eu-west-2.amazonaws.com/eu-west-2_abc123",
        UserPoolId = "eu-west-2_abc123",
      });
      _cognitoService = new CognitoService(_mockCognitoClient.Object, _options, _mockLogger.Object);
    }

    [Test]
    public async Task should_return_cognito_user_info_with_sub_and_preferred_username()
    {
      // arrange
      var response = new AdminGetUserResponse
      {
        UserAttributes = new List<AttributeType>
        {
          new AttributeType { Name = "sub", Value = "user-sub-123" },
          new AttributeType { Name = "preferred_username", Value = "testuser" },
          new AttributeType { Name = "email", Value = "user@example.com" },
        },
      };

      _mockCognitoClient
        .Setup(c => c.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(response);

      // act
      var result = await _cognitoService.GetUserByEmailAsync("user@example.com");

      // assert
      result.Should().NotBeNull();
      result.Sub.Should().Be("user-sub-123");
      result.PreferredUsername.Should().Be("testuser");
    }

    [Test]
    public async Task should_pass_email_as_username_and_correct_user_pool_id()
    {
      // arrange
      AdminGetUserRequest capturedRequest = null;

      _mockCognitoClient
        .Setup(c => c.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), It.IsAny<CancellationToken>()))
        .Callback<AdminGetUserRequest, CancellationToken>((req, _) => capturedRequest = req)
        .ReturnsAsync(new AdminGetUserResponse
        {
          UserAttributes = new List<AttributeType>
          {
            new AttributeType { Name = "sub", Value = "user-sub-123" },
            new AttributeType { Name = "preferred_username", Value = "testuser" },
          },
        });

      // act
      await _cognitoService.GetUserByEmailAsync("user@example.com");

      // assert
      capturedRequest.Username.Should().Be("user@example.com");
      capturedRequest.UserPoolId.Should().Be("eu-west-2_abc123");
    }

    [Test]
    public async Task should_return_null_when_user_not_found()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new UserNotFoundException("Not found"));

      // act
      var result = await _cognitoService.GetUserByEmailAsync("nobody@example.com");

      // assert
      result.Should().BeNull();
    }

    [Test]
    public async Task should_rethrow_non_user_not_found_exceptions()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Something went wrong"));

      // act
      Func<Task> act = () => _cognitoService.GetUserByEmailAsync("user@example.com");

      // assert
      await act.Should().ThrowAsync<Exception>().WithMessage("Something went wrong");
    }
  }

  [TestFixture]
  public class CognitoServiceGetPreferredUsernameTests
  {
    private Mock<IAmazonCognitoIdentityProvider> _mockCognitoClient;
    private Mock<ILogger<CognitoService>> _mockLogger;
    private IOptions<CognitoOptions> _options;
    private CognitoService _cognitoService;

    [SetUp]
    public void SetUp()
    {
      _mockCognitoClient = new Mock<IAmazonCognitoIdentityProvider>();
      _mockLogger = new Mock<ILogger<CognitoService>>();
      _options = Options.Create(new CognitoOptions
      {
        ClientId = "test-client-id",
        ClientSecret = "test-client-secret",
        Authority = "https://cognito-idp.eu-west-2.amazonaws.com/eu-west-2_abc123",
        UserPoolId = "eu-west-2_abc123",
      });
      _cognitoService = new CognitoService(_mockCognitoClient.Object, _options, _mockLogger.Object);
    }

    [Test]
    public async Task should_return_preferred_username_from_cognito()
    {
      // arrange
      var response = new AdminGetUserResponse
      {
        UserAttributes = new List<AttributeType>
        {
          new AttributeType { Name = "preferred_username", Value = "testuser" },
        },
      };

      _mockCognitoClient
        .Setup(c => c.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(response);

      // act
      var result = await _cognitoService.GetPreferredUsernameAsync("user-sub-123");

      // assert
      result.Should().Be("testuser");
    }

    [Test]
    public async Task should_return_unknown_user_when_attribute_not_found()
    {
      // arrange
      var response = new AdminGetUserResponse
      {
        UserAttributes = new List<AttributeType>(),
      };

      _mockCognitoClient
        .Setup(c => c.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(response);

      // act
      var result = await _cognitoService.GetPreferredUsernameAsync("user-sub-123");

      // assert
      result.Should().Be("Unknown User");
    }

    [Test]
    public async Task should_return_unknown_user_when_user_not_found()
    {
      // arrange
      _mockCognitoClient
        .Setup(c => c.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new UserNotFoundException("Not found"));

      // act
      var result = await _cognitoService.GetPreferredUsernameAsync("nobody");

      // assert
      result.Should().Be("Unknown User");
    }
  }
}
