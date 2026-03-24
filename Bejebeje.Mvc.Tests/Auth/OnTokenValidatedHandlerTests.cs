namespace Bejebeje.Mvc.Tests.Auth
{
  using System;
  using System.Collections.Generic;
  using System.Security.Claims;
  using System.Threading.Tasks;
  using Bejebeje.Domain;
  using Bejebeje.Services.Services.Interfaces;
  using FluentAssertions;
  using Microsoft.AspNetCore.Authentication.OpenIdConnect;
  using Microsoft.AspNetCore.Http;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using Microsoft.IdentityModel.Protocols.OpenIdConnect;
  using Moq;
  using Mvc.Auth;
  using NUnit.Framework;

  [TestFixture]
  public class OnTokenValidatedHandlerTests
  {
    private Mock<IBbPointsService> _mockPointsService;
    private Mock<ICognitoService> _mockCognitoService;
    private Mock<ILogger<OnTokenValidatedHandler>> _mockLogger;
    private OnTokenValidatedHandler _handler;

    [SetUp]
    public void SetUp()
    {
      _mockPointsService = new Mock<IBbPointsService>();
      _mockCognitoService = new Mock<ICognitoService>();
      _mockLogger = new Mock<ILogger<OnTokenValidatedHandler>>();
      _handler = new OnTokenValidatedHandler(
        _mockPointsService.Object,
        _mockCognitoService.Object,
        _mockLogger.Object);
    }

    [Test]
    public async Task should_call_ensure_user_exists_with_sub_and_preferred_username_claims()
    {
      // arrange
      var claims = new List<Claim>
      {
        new Claim("sub", "user-123"),
        new Claim("preferred_username", "testuser"),
      };

      var principal = CreatePrincipal(claims);

      _mockPointsService
        .Setup(s => s.EnsureUserExistsAsync("user-123", "testuser"))
        .ReturnsAsync(new User { CognitoUserId = "user-123", Username = "testuser" });

      // act
      await _handler.HandleAsync(principal);

      // assert
      _mockPointsService.Verify(
        s => s.EnsureUserExistsAsync("user-123", "testuser"),
        Times.Once);
    }

    [Test]
    public async Task should_fallback_to_cognito_service_when_preferred_username_claim_missing()
    {
      // arrange
      var claims = new List<Claim>
      {
        new Claim("sub", "user-456"),
      };

      var principal = CreatePrincipal(claims);

      _mockCognitoService
        .Setup(s => s.GetPreferredUsernameAsync("user-456"))
        .ReturnsAsync("resolveduser");

      _mockPointsService
        .Setup(s => s.EnsureUserExistsAsync("user-456", "resolveduser"))
        .ReturnsAsync(new User { CognitoUserId = "user-456", Username = "resolveduser" });

      // act
      await _handler.HandleAsync(principal);

      // assert
      _mockCognitoService.Verify(
        s => s.GetPreferredUsernameAsync("user-456"),
        Times.Once);

      _mockPointsService.Verify(
        s => s.EnsureUserExistsAsync("user-456", "resolveduser"),
        Times.Once);
    }

    [Test]
    public async Task should_not_throw_when_sub_claim_missing()
    {
      // arrange
      var claims = new List<Claim>();
      var principal = CreatePrincipal(claims);

      // act
      Func<Task> act = () => _handler.HandleAsync(principal);

      // assert
      await act.Should().NotThrowAsync();
      _mockPointsService.Verify(
        s => s.EnsureUserExistsAsync(It.IsAny<string>(), It.IsAny<string>()),
        Times.Never);
    }

    [Test]
    public async Task should_not_throw_when_ensure_user_exists_fails()
    {
      // arrange
      var claims = new List<Claim>
      {
        new Claim("sub", "user-789"),
        new Claim("preferred_username", "testuser"),
      };

      var principal = CreatePrincipal(claims);

      _mockPointsService
        .Setup(s => s.EnsureUserExistsAsync("user-789", "testuser"))
        .ThrowsAsync(new Exception("database error"));

      // act
      Func<Task> act = () => _handler.HandleAsync(principal);

      // assert
      await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task should_not_throw_when_cognito_fallback_fails()
    {
      // arrange
      var claims = new List<Claim>
      {
        new Claim("sub", "user-fallback"),
      };

      var principal = CreatePrincipal(claims);

      _mockCognitoService
        .Setup(s => s.GetPreferredUsernameAsync("user-fallback"))
        .ThrowsAsync(new Exception("cognito error"));

      // act
      Func<Task> act = () => _handler.HandleAsync(principal);

      // assert
      await act.Should().NotThrowAsync();
      _mockPointsService.Verify(
        s => s.EnsureUserExistsAsync(It.IsAny<string>(), It.IsAny<string>()),
        Times.Never);
    }

    [Test]
    public async Task should_use_cognito_user_id_prefix_when_cognito_fallback_fails()
    {
      // arrange
      var claims = new List<Claim>
      {
        new Claim("sub", "abcdefghij-1234"),
      };

      var principal = CreatePrincipal(claims);

      _mockCognitoService
        .Setup(s => s.GetPreferredUsernameAsync("abcdefghij-1234"))
        .ThrowsAsync(new Exception("cognito error"));

      // act
      await _handler.HandleAsync(principal);

      // assert
      // when cognito fails, we can't resolve username, so we should not call ensure user exists
      // (we don't want to create users with fallback usernames from the login flow)
      _mockPointsService.Verify(
        s => s.EnsureUserExistsAsync(It.IsAny<string>(), It.IsAny<string>()),
        Times.Never);
    }

    private static ClaimsPrincipal CreatePrincipal(List<Claim> claims)
    {
      var identity = new ClaimsIdentity(claims, "TestAuth");
      return new ClaimsPrincipal(identity);
    }
  }
}
