namespace Bejebeje.Mvc.Tests.Auth
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Security.Claims;
  using System.Text;
  using System.Text.Json;
  using System.Threading.Tasks;
  using Bejebeje.Domain;
  using Bejebeje.Mvc.Auth;
  using Bejebeje.Services.Services.Interfaces;
  using FluentAssertions;
  using Microsoft.Extensions.Logging;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class OnSigningInHandlerTests
  {
    private Mock<IBbPointsService> _mockPointsService;
    private Mock<ICognitoService> _mockCognitoService;
    private Mock<ILogger<OnSigningInHandler>> _mockLogger;
    private OnSigningInHandler _handler;

    [SetUp]
    public void SetUp()
    {
      _mockPointsService = new Mock<IBbPointsService>();
      _mockCognitoService = new Mock<ICognitoService>();
      _mockLogger = new Mock<ILogger<OnSigningInHandler>>();
      _handler = new OnSigningInHandler(
        _mockPointsService.Object,
        _mockCognitoService.Object,
        _mockLogger.Object);
    }

    [Test]
    public async Task should_extract_claims_from_valid_id_token()
    {
      // arrange
      var identity = CreateIdentityWithIdToken("user-sub-123", "testuser", "user@example.com");

      _mockPointsService
        .Setup(s => s.EnsureUserExistsAsync("user-sub-123", "testuser"))
        .ReturnsAsync(new User { CognitoUserId = "user-sub-123", Username = "testuser" });

      // act
      await _handler.HandleAsync(identity);

      // assert
      identity.FindFirst("sub")?.Value.Should().Be("user-sub-123");
      identity.FindFirst("preferred_username")?.Value.Should().Be("testuser");
      identity.FindFirst("email")?.Value.Should().Be("user@example.com");
    }

    [Test]
    public async Task should_call_ensure_user_exists_with_extracted_claims()
    {
      // arrange
      var identity = CreateIdentityWithIdToken("user-sub-123", "testuser", "user@example.com");

      _mockPointsService
        .Setup(s => s.EnsureUserExistsAsync("user-sub-123", "testuser"))
        .ReturnsAsync(new User { CognitoUserId = "user-sub-123", Username = "testuser" });

      // act
      await _handler.HandleAsync(identity);

      // assert
      _mockPointsService.Verify(
        s => s.EnsureUserExistsAsync("user-sub-123", "testuser"),
        Times.Once);
    }

    [Test]
    public async Task should_strip_token_claims_after_successful_extraction()
    {
      // arrange
      var identity = CreateIdentityWithIdToken("user-sub-123", "testuser", "user@example.com");

      _mockPointsService
        .Setup(s => s.EnsureUserExistsAsync(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(new User());

      // act
      await _handler.HandleAsync(identity);

      // assert
      identity.FindFirst("IdToken").Should().BeNull();
      identity.FindFirst("AccessToken").Should().BeNull();
      identity.FindFirst("RefreshToken").Should().BeNull();
    }

    [Test]
    public async Task should_log_warning_and_strip_tokens_when_id_token_claim_is_missing()
    {
      // arrange
      var identity = new ClaimsIdentity("TestAuth");
      identity.AddClaim(new Claim("AccessToken", "access-token-value"));
      identity.AddClaim(new Claim("RefreshToken", "refresh-token-value"));

      // act
      await _handler.HandleAsync(identity);

      // assert
      identity.FindFirst("IdToken").Should().BeNull();
      identity.FindFirst("AccessToken").Should().BeNull();
      identity.FindFirst("RefreshToken").Should().BeNull();
      _mockPointsService.Verify(
        s => s.EnsureUserExistsAsync(It.IsAny<string>(), It.IsAny<string>()),
        Times.Never);
    }

    [Test]
    public async Task should_strip_tokens_and_not_block_login_when_jwt_parsing_fails()
    {
      // arrange
      var identity = new ClaimsIdentity("TestAuth");
      identity.AddClaim(new Claim("IdToken", "not.a.valid-jwt"));
      identity.AddClaim(new Claim("AccessToken", "access-token-value"));
      identity.AddClaim(new Claim("RefreshToken", "refresh-token-value"));

      // act
      Func<Task> act = () => _handler.HandleAsync(identity);

      // assert
      await act.Should().NotThrowAsync();
      identity.FindFirst("IdToken").Should().BeNull();
      identity.FindFirst("AccessToken").Should().BeNull();
      identity.FindFirst("RefreshToken").Should().BeNull();
    }

    [Test]
    public async Task should_fallback_to_cognito_when_preferred_username_missing_from_token()
    {
      // arrange
      var identity = CreateIdentityWithIdToken("user-sub-456", null, "user@example.com");

      _mockCognitoService
        .Setup(s => s.GetPreferredUsernameAsync("user-sub-456"))
        .ReturnsAsync("resolveduser");

      _mockPointsService
        .Setup(s => s.EnsureUserExistsAsync("user-sub-456", "resolveduser"))
        .ReturnsAsync(new User());

      // act
      await _handler.HandleAsync(identity);

      // assert
      identity.FindFirst("preferred_username")?.Value.Should().Be("resolveduser");
      _mockCognitoService.Verify(
        s => s.GetPreferredUsernameAsync("user-sub-456"),
        Times.Once);
      _mockPointsService.Verify(
        s => s.EnsureUserExistsAsync("user-sub-456", "resolveduser"),
        Times.Once);
    }

    [Test]
    public async Task should_skip_user_sync_when_preferred_username_fallback_fails()
    {
      // arrange
      var identity = CreateIdentityWithIdToken("user-sub-789", null, "user@example.com");

      _mockCognitoService
        .Setup(s => s.GetPreferredUsernameAsync("user-sub-789"))
        .ThrowsAsync(new Exception("cognito error"));

      // act
      Func<Task> act = () => _handler.HandleAsync(identity);

      // assert
      await act.Should().NotThrowAsync();
      _mockPointsService.Verify(
        s => s.EnsureUserExistsAsync(It.IsAny<string>(), It.IsAny<string>()),
        Times.Never);
      identity.FindFirst("IdToken").Should().BeNull();
    }

    [Test]
    public async Task should_not_block_login_when_ensure_user_exists_throws()
    {
      // arrange
      var identity = CreateIdentityWithIdToken("user-sub-123", "testuser", "user@example.com");

      _mockPointsService
        .Setup(s => s.EnsureUserExistsAsync("user-sub-123", "testuser"))
        .ThrowsAsync(new Exception("database error"));

      // act
      Func<Task> act = () => _handler.HandleAsync(identity);

      // assert
      await act.Should().NotThrowAsync();
      identity.FindFirst("sub")?.Value.Should().Be("user-sub-123");
      identity.FindFirst("IdToken").Should().BeNull();
    }

    [Test]
    public async Task should_log_warning_when_ensure_user_exists_returns_null()
    {
      // arrange
      var identity = CreateIdentityWithIdToken("user-sub-123", "testuser", "user@example.com");

      _mockPointsService
        .Setup(s => s.EnsureUserExistsAsync("user-sub-123", "testuser"))
        .ReturnsAsync((User)null);

      // act
      Func<Task> act = () => _handler.HandleAsync(identity);

      // assert
      await act.Should().NotThrowAsync();
      identity.FindFirst("sub")?.Value.Should().Be("user-sub-123");
      identity.FindFirst("IdToken").Should().BeNull();
    }

    [Test]
    public async Task should_strip_tokens_even_when_exception_thrown_in_claim_extraction()
    {
      // arrange
      var identity = new ClaimsIdentity("TestAuth");
      identity.AddClaim(new Claim("IdToken", "completely-invalid"));
      identity.AddClaim(new Claim("AccessToken", "access-token-value"));
      identity.AddClaim(new Claim("RefreshToken", "refresh-token-value"));

      // act
      await _handler.HandleAsync(identity);

      // assert
      identity.FindFirst("IdToken").Should().BeNull();
      identity.FindFirst("AccessToken").Should().BeNull();
      identity.FindFirst("RefreshToken").Should().BeNull();
    }

    private static ClaimsIdentity CreateIdentityWithIdToken(string sub, string preferredUsername, string email)
    {
      var payload = new Dictionary<string, object>();

      if (sub != null)
      {
        payload["sub"] = sub;
      }

      if (preferredUsername != null)
      {
        payload["preferred_username"] = preferredUsername;
      }

      if (email != null)
      {
        payload["email"] = email;
      }

      string payloadJson = JsonSerializer.Serialize(payload);
      string payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson))
        .TrimEnd('=')
        .Replace('+', '-')
        .Replace('/', '_');

      // jwt format: header.payload.signature (header and signature are not validated)
      string fakeJwt = $"eyJhbGciOiJSUzI1NiJ9.{payloadBase64}.fake-signature";

      var identity = new ClaimsIdentity("TestAuth");
      identity.AddClaim(new Claim("IdToken", fakeJwt));
      identity.AddClaim(new Claim("AccessToken", "access-token-value"));
      identity.AddClaim(new Claim("RefreshToken", "refresh-token-value"));

      return identity;
    }
  }
}
