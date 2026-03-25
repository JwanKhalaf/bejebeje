namespace Bejebeje.Services.Tests.Services
{
  using Bejebeje.Services.Services;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class ResetPasswordResultTests
  {
    [Test]
    public void should_create_successful_result()
    {
      var result = ResetPasswordResult.Succeed();

      result.Success.Should().BeTrue();
      result.ErrorType.Should().BeNull();
      result.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void should_create_failed_result_with_code_mismatch_error()
    {
      var result = ResetPasswordResult.Fail(ResetErrorType.CodeMismatch, "Invalid confirmation code. Please check the code and try again.");

      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(ResetErrorType.CodeMismatch);
      result.ErrorMessage.Should().Be("Invalid confirmation code. Please check the code and try again.");
    }

    [Test]
    public void should_create_failed_result_with_expired_code_error()
    {
      var result = ResetPasswordResult.Fail(ResetErrorType.ExpiredCode, "The confirmation code has expired. Please request a new one.");

      result.ErrorType.Should().Be(ResetErrorType.ExpiredCode);
    }

    [Test]
    public void should_create_failed_result_with_invalid_password_error()
    {
      var result = ResetPasswordResult.Fail(ResetErrorType.InvalidPassword, "Password does not meet the required criteria.");

      result.ErrorType.Should().Be(ResetErrorType.InvalidPassword);
    }

    [Test]
    public void should_create_failed_result_with_user_not_found_error()
    {
      var result = ResetPasswordResult.Fail(ResetErrorType.UserNotFound, "Unable to reset password. Please try again.");

      result.ErrorType.Should().Be(ResetErrorType.UserNotFound);
    }

    [Test]
    public void should_create_failed_result_with_unexpected_error()
    {
      var result = ResetPasswordResult.Fail(ResetErrorType.Unexpected, "Unable to reset password. Please try again.");

      result.ErrorType.Should().Be(ResetErrorType.Unexpected);
    }
  }

  [TestFixture]
  public class ResetErrorTypeTests
  {
    [Test]
    public void should_have_code_mismatch_value()
    {
      ResetErrorType.CodeMismatch.Should().BeDefined();
    }

    [Test]
    public void should_have_expired_code_value()
    {
      ResetErrorType.ExpiredCode.Should().BeDefined();
    }

    [Test]
    public void should_have_invalid_password_value()
    {
      ResetErrorType.InvalidPassword.Should().BeDefined();
    }

    [Test]
    public void should_have_user_not_found_value()
    {
      ResetErrorType.UserNotFound.Should().BeDefined();
    }

    [Test]
    public void should_have_unexpected_value()
    {
      ResetErrorType.Unexpected.Should().BeDefined();
    }
  }
}
