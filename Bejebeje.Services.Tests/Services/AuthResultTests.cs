namespace Bejebeje.Services.Tests.Services
{
  using Bejebeje.Services.Services;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class AuthResultTests
  {
    [Test]
    public void should_create_successful_result_with_tokens()
    {
      var result = AuthResult.Succeed("id-token", "access-token", "refresh-token");

      result.Success.Should().BeTrue();
      result.IdToken.Should().Be("id-token");
      result.AccessToken.Should().Be("access-token");
      result.RefreshToken.Should().Be("refresh-token");
      result.ErrorType.Should().BeNull();
      result.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void should_create_failed_result_with_error_type_and_message()
    {
      var result = AuthResult.Fail(AuthErrorType.InvalidCredentials, "Invalid email or password. Please try again.");

      result.Success.Should().BeFalse();
      result.IdToken.Should().BeNull();
      result.AccessToken.Should().BeNull();
      result.RefreshToken.Should().BeNull();
      result.ErrorType.Should().Be(AuthErrorType.InvalidCredentials);
      result.ErrorMessage.Should().Be("Invalid email or password. Please try again.");
    }

    [Test]
    public void should_have_user_not_confirmed_error_type()
    {
      var result = AuthResult.Fail(AuthErrorType.UserNotConfirmed, "Please confirm your email.");

      result.ErrorType.Should().Be(AuthErrorType.UserNotConfirmed);
    }

    [Test]
    public void should_have_too_many_requests_error_type()
    {
      var result = AuthResult.Fail(AuthErrorType.TooManyRequests, "Too many requests.");

      result.ErrorType.Should().Be(AuthErrorType.TooManyRequests);
    }

    [Test]
    public void should_have_unexpected_error_type()
    {
      var result = AuthResult.Fail(AuthErrorType.Unexpected, "An unexpected error occurred.");

      result.ErrorType.Should().Be(AuthErrorType.Unexpected);
    }
  }

  [TestFixture]
  public class AuthErrorTypeTests
  {
    [Test]
    public void should_have_invalid_credentials_value()
    {
      AuthErrorType.InvalidCredentials.Should().BeDefined();
    }

    [Test]
    public void should_have_user_not_confirmed_value()
    {
      AuthErrorType.UserNotConfirmed.Should().BeDefined();
    }

    [Test]
    public void should_have_too_many_requests_value()
    {
      AuthErrorType.TooManyRequests.Should().BeDefined();
    }

    [Test]
    public void should_have_unexpected_value()
    {
      AuthErrorType.Unexpected.Should().BeDefined();
    }
  }
}
