namespace Bejebeje.Services.Tests.Services
{
  using Bejebeje.Services.Services;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class ForgotPasswordResultTests
  {
    [Test]
    public void should_create_successful_result()
    {
      var result = ForgotPasswordResult.Succeed();

      result.Success.Should().BeTrue();
      result.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void should_create_failed_result_with_error_message()
    {
      var result = ForgotPasswordResult.Fail("An unexpected error occurred. Please try again.");

      result.Success.Should().BeFalse();
      result.ErrorMessage.Should().Be("An unexpected error occurred. Please try again.");
    }
  }
}
