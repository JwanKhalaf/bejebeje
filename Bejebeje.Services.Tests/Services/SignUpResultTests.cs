namespace Bejebeje.Services.Tests.Services
{
  using Bejebeje.Services.Services;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class SignUpResultTests
  {
    [Test]
    public void should_create_successful_result()
    {
      var result = SignUpResult.Succeed();

      result.Success.Should().BeTrue();
      result.ErrorType.Should().BeNull();
      result.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void should_create_failed_result_with_username_exists_error()
    {
      var result = SignUpResult.Fail(SignUpErrorType.UsernameExists, "An account with this email already exists.");

      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(SignUpErrorType.UsernameExists);
      result.ErrorMessage.Should().Be("An account with this email already exists.");
    }

    [Test]
    public void should_create_failed_result_with_invalid_password_error()
    {
      var result = SignUpResult.Fail(SignUpErrorType.InvalidPassword, "Password does not meet the required criteria.");

      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(SignUpErrorType.InvalidPassword);
    }

    [Test]
    public void should_create_failed_result_with_invalid_parameter_error()
    {
      var result = SignUpResult.Fail(SignUpErrorType.InvalidParameter, "Please check your input and try again.");

      result.ErrorType.Should().Be(SignUpErrorType.InvalidParameter);
    }

    [Test]
    public void should_create_failed_result_with_unexpected_error()
    {
      var result = SignUpResult.Fail(SignUpErrorType.Unexpected, "An unexpected error occurred.");

      result.ErrorType.Should().Be(SignUpErrorType.Unexpected);
    }
  }

  [TestFixture]
  public class SignUpErrorTypeTests
  {
    [Test]
    public void should_have_username_exists_value()
    {
      SignUpErrorType.UsernameExists.Should().BeDefined();
    }

    [Test]
    public void should_have_invalid_password_value()
    {
      SignUpErrorType.InvalidPassword.Should().BeDefined();
    }

    [Test]
    public void should_have_invalid_parameter_value()
    {
      SignUpErrorType.InvalidParameter.Should().BeDefined();
    }

    [Test]
    public void should_have_unexpected_value()
    {
      SignUpErrorType.Unexpected.Should().BeDefined();
    }
  }
}
