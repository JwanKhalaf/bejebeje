namespace Bejebeje.Mvc.Tests.Models
{
  using Bejebeje.Models.Account;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class LoginViewModelTests
  {
    [Test]
    public void should_have_email_property()
    {
      var vm = new LoginViewModel { Email = "user@example.com" };
      vm.Email.Should().Be("user@example.com");
    }

    [Test]
    public void should_have_password_property()
    {
      var vm = new LoginViewModel { Password = "secret" };
      vm.Password.Should().Be("secret");
    }

    [Test]
    public void should_have_remember_me_property()
    {
      var vm = new LoginViewModel { RememberMe = true };
      vm.RememberMe.Should().BeTrue();
    }

    [Test]
    public void should_have_return_url_property()
    {
      var vm = new LoginViewModel { ReturnUrl = "/profile" };
      vm.ReturnUrl.Should().Be("/profile");
    }

    [Test]
    public void should_have_error_message_property()
    {
      var vm = new LoginViewModel { ErrorMessage = "Invalid credentials" };
      vm.ErrorMessage.Should().Be("Invalid credentials");
    }
  }

  [TestFixture]
  public class SignupViewModelTests
  {
    [Test]
    public void should_have_email_property()
    {
      var vm = new SignupViewModel { Email = "user@example.com" };
      vm.Email.Should().Be("user@example.com");
    }

    [Test]
    public void should_have_username_property()
    {
      var vm = new SignupViewModel { Username = "testuser" };
      vm.Username.Should().Be("testuser");
    }

    [Test]
    public void should_have_password_property()
    {
      var vm = new SignupViewModel { Password = "secret" };
      vm.Password.Should().Be("secret");
    }

    [Test]
    public void should_have_error_message_property()
    {
      var vm = new SignupViewModel { ErrorMessage = "Email taken" };
      vm.ErrorMessage.Should().Be("Email taken");
    }
  }

  [TestFixture]
  public class ConfirmViewModelTests
  {
    [Test]
    public void should_have_email_property()
    {
      var vm = new ConfirmViewModel { Email = "user@example.com" };
      vm.Email.Should().Be("user@example.com");
    }

    [Test]
    public void should_have_code_property()
    {
      var vm = new ConfirmViewModel { Code = "123456" };
      vm.Code.Should().Be("123456");
    }

    [Test]
    public void should_have_error_message_property()
    {
      var vm = new ConfirmViewModel { ErrorMessage = "Bad code" };
      vm.ErrorMessage.Should().Be("Bad code");
    }

    [Test]
    public void should_have_success_message_property()
    {
      var vm = new ConfirmViewModel { SuccessMessage = "Code resent" };
      vm.SuccessMessage.Should().Be("Code resent");
    }
  }

  [TestFixture]
  public class ForgottenPasswordViewModelTests
  {
    [Test]
    public void should_have_email_property()
    {
      var vm = new ForgottenPasswordViewModel { Email = "user@example.com" };
      vm.Email.Should().Be("user@example.com");
    }

    [Test]
    public void should_have_error_message_property()
    {
      var vm = new ForgottenPasswordViewModel { ErrorMessage = "Error" };
      vm.ErrorMessage.Should().Be("Error");
    }
  }

  [TestFixture]
  public class ResetPasswordViewModelTests
  {
    [Test]
    public void should_have_email_property()
    {
      var vm = new ResetPasswordViewModel { Email = "user@example.com" };
      vm.Email.Should().Be("user@example.com");
    }

    [Test]
    public void should_have_code_property()
    {
      var vm = new ResetPasswordViewModel { Code = "123456" };
      vm.Code.Should().Be("123456");
    }

    [Test]
    public void should_have_new_password_property()
    {
      var vm = new ResetPasswordViewModel { NewPassword = "NewPass123!" };
      vm.NewPassword.Should().Be("NewPass123!");
    }

    [Test]
    public void should_have_confirm_password_property()
    {
      var vm = new ResetPasswordViewModel { ConfirmPassword = "NewPass123!" };
      vm.ConfirmPassword.Should().Be("NewPass123!");
    }

    [Test]
    public void should_have_error_message_property()
    {
      var vm = new ResetPasswordViewModel { ErrorMessage = "Error" };
      vm.ErrorMessage.Should().Be("Error");
    }
  }
}
