namespace Bejebeje.Services.Tests.Services
{
  using Bejebeje.Services.Services;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class ConfirmResultTests
  {
    [Test]
    public void should_create_successful_result()
    {
      var result = ConfirmResult.Succeed();

      result.Success.Should().BeTrue();
      result.ErrorType.Should().BeNull();
      result.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void should_create_failed_result_with_code_mismatch_error()
    {
      var result = ConfirmResult.Fail(ConfirmErrorType.CodeMismatch, "Invalid confirmation code. Please check the code and try again.");

      result.Success.Should().BeFalse();
      result.ErrorType.Should().Be(ConfirmErrorType.CodeMismatch);
      result.ErrorMessage.Should().Be("Invalid confirmation code. Please check the code and try again.");
    }

    [Test]
    public void should_create_failed_result_with_expired_code_error()
    {
      var result = ConfirmResult.Fail(ConfirmErrorType.ExpiredCode, "The confirmation code has expired. Please request a new one.");

      result.ErrorType.Should().Be(ConfirmErrorType.ExpiredCode);
    }

    [Test]
    public void should_create_failed_result_with_alias_exists_error()
    {
      var result = ConfirmResult.Fail(ConfirmErrorType.AliasExists, "An account with this email is already confirmed.");

      result.ErrorType.Should().Be(ConfirmErrorType.AliasExists);
    }

    [Test]
    public void should_create_failed_result_with_unexpected_error()
    {
      var result = ConfirmResult.Fail(ConfirmErrorType.Unexpected, "An unexpected error occurred. Please try again.");

      result.ErrorType.Should().Be(ConfirmErrorType.Unexpected);
    }
  }

  [TestFixture]
  public class ConfirmErrorTypeTests
  {
    [Test]
    public void should_have_code_mismatch_value()
    {
      ConfirmErrorType.CodeMismatch.Should().BeDefined();
    }

    [Test]
    public void should_have_expired_code_value()
    {
      ConfirmErrorType.ExpiredCode.Should().BeDefined();
    }

    [Test]
    public void should_have_alias_exists_value()
    {
      ConfirmErrorType.AliasExists.Should().BeDefined();
    }

    [Test]
    public void should_have_unexpected_value()
    {
      ConfirmErrorType.Unexpected.Should().BeDefined();
    }
  }
}
