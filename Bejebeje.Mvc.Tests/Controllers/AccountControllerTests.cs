namespace Bejebeje.Mvc.Tests.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Security.Claims;
  using System.Threading.Tasks;
  using Bejebeje.Shared.Domain;
  using Bejebeje.Models.Account;
  using Bejebeje.Mvc.Controllers;
  using Bejebeje.Services.Services;
  using Bejebeje.Services.Services.Interfaces;
  using FluentAssertions;
  using Microsoft.AspNetCore.Authentication;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Mvc.Routing;
  using Microsoft.AspNetCore.Mvc.ViewFeatures;
  using Microsoft.Extensions.Logging;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class AccountControllerLoginGetTests
  {
    private AccountController _controller;

    [SetUp]
    public void SetUp()
    {
      _controller = AccountControllerTestHelper.CreateController();
    }

    [Test]
    public void should_return_view_with_empty_login_view_model()
    {
      // act
      var result = _controller.Login();

      // assert
      var viewResult = result.Should().BeOfType<ViewResult>().Subject;
      var model = viewResult.Model.Should().BeOfType<LoginViewModel>().Subject;
      model.Email.Should().BeNull();
      model.Password.Should().BeNull();
      model.RememberMe.Should().BeFalse();
    }

    [Test]
    public void should_populate_return_url_from_query_parameter()
    {
      // act
      var result = _controller.Login("/profile");

      // assert
      var viewResult = result.Should().BeOfType<ViewResult>().Subject;
      var model = viewResult.Model.Should().BeOfType<LoginViewModel>().Subject;
      model.ReturnUrl.Should().Be("/profile");
    }
  }

  [TestFixture]
  public class AccountControllerLoginPostTests
  {
    private Mock<IAuthService> _mockAuthService;
    private Mock<IBbPointsService> _mockPointsService;
    private Mock<ICognitoService> _mockCognitoService;
    private Mock<ILogger<AccountController>> _mockLogger;
    private AccountController _controller;
    private Mock<IAuthenticationService> _mockAuthenticationService;

    [SetUp]
    public void SetUp()
    {
      _mockAuthService = new Mock<IAuthService>();
      _mockPointsService = new Mock<IBbPointsService>();
      _mockCognitoService = new Mock<ICognitoService>();
      _mockLogger = new Mock<ILogger<AccountController>>();
      _mockAuthenticationService = new Mock<IAuthenticationService>();

      _controller = new AccountController(
        _mockAuthService.Object,
        _mockPointsService.Object,
        _mockCognitoService.Object,
        _mockLogger.Object);

      SetupControllerContext(_controller);
    }

    [Test]
    public async Task should_return_view_with_error_on_failed_auth()
    {
      // arrange
      var model = new LoginViewModel { Email = "user@example.com", Password = "wrong" };

      _mockAuthService
        .Setup(s => s.AuthenticateAsync("user@example.com", "wrong", It.IsAny<string>()))
        .ReturnsAsync(AuthResult.Fail(AuthErrorType.InvalidCredentials, "Invalid email or password. Please try again."));

      // act
      var result = await _controller.Login(model);

      // assert
      var viewResult = result.Should().BeOfType<ViewResult>().Subject;
      var vm = viewResult.Model.Should().BeOfType<LoginViewModel>().Subject;
      vm.ErrorMessage.Should().Be("Invalid email or password. Please try again.");
      vm.Email.Should().Be("user@example.com");
      vm.Password.Should().BeNull();
    }

    [Test]
    public async Task should_sign_in_and_redirect_to_root_on_success()
    {
      // arrange
      var model = new LoginViewModel { Email = "user@example.com", Password = "Password123!" };

      _mockAuthService
        .Setup(s => s.AuthenticateAsync("user@example.com", "Password123!", It.IsAny<string>()))
        .ReturnsAsync(AuthResult.Succeed("id-token", "access-token", "refresh-token"));

      // act
      var result = await _controller.Login(model);

      // assert
      var redirectResult = result.Should().BeOfType<RedirectResult>().Subject;
      redirectResult.Url.Should().Be("/");
    }

    [Test]
    public async Task should_redirect_to_local_return_url_on_success()
    {
      // arrange
      var model = new LoginViewModel
      {
        Email = "user@example.com",
        Password = "Password123!",
        ReturnUrl = "/profile",
      };

      _mockAuthService
        .Setup(s => s.AuthenticateAsync("user@example.com", "Password123!", It.IsAny<string>()))
        .ReturnsAsync(AuthResult.Succeed("id-token", "access-token", "refresh-token"));

      SetupUrlHelper(isLocalUrl: true);

      // act
      var result = await _controller.Login(model);

      // assert
      var redirectResult = result.Should().BeOfType<RedirectResult>().Subject;
      redirectResult.Url.Should().Be("/profile");
    }

    [Test]
    public async Task should_redirect_to_root_when_return_url_is_external()
    {
      // arrange
      var model = new LoginViewModel
      {
        Email = "user@example.com",
        Password = "Password123!",
        ReturnUrl = "https://evil.com",
      };

      _mockAuthService
        .Setup(s => s.AuthenticateAsync("user@example.com", "Password123!", It.IsAny<string>()))
        .ReturnsAsync(AuthResult.Succeed("id-token", "access-token", "refresh-token"));

      SetupUrlHelper(isLocalUrl: false);

      // act
      var result = await _controller.Login(model);

      // assert
      var redirectResult = result.Should().BeOfType<RedirectResult>().Subject;
      redirectResult.Url.Should().Be("/");
    }

    [Test]
    public async Task should_call_sign_in_with_persistent_cookie_when_remember_me_checked()
    {
      // arrange
      var model = new LoginViewModel
      {
        Email = "user@example.com",
        Password = "Password123!",
        RememberMe = true,
      };

      AuthenticationProperties capturedProperties = null;

      _mockAuthService
        .Setup(s => s.AuthenticateAsync("user@example.com", "Password123!", It.IsAny<string>()))
        .ReturnsAsync(AuthResult.Succeed("id-token", "access-token", "refresh-token"));

      _mockAuthenticationService
        .Setup(a => a.SignInAsync(
          It.IsAny<HttpContext>(),
          It.IsAny<string>(),
          It.IsAny<ClaimsPrincipal>(),
          It.IsAny<AuthenticationProperties>()))
        .Callback<HttpContext, string, ClaimsPrincipal, AuthenticationProperties>((_, _, _, props) => capturedProperties = props)
        .Returns(Task.CompletedTask);

      // act
      await _controller.Login(model);

      // assert
      capturedProperties.Should().NotBeNull();
      capturedProperties.IsPersistent.Should().BeTrue();
      capturedProperties.ExpiresUtc.Should().NotBeNull();
    }

    [Test]
    public async Task should_call_sign_in_with_session_cookie_when_remember_me_unchecked()
    {
      // arrange
      var model = new LoginViewModel
      {
        Email = "user@example.com",
        Password = "Password123!",
        RememberMe = false,
      };

      AuthenticationProperties capturedProperties = null;

      _mockAuthService
        .Setup(s => s.AuthenticateAsync("user@example.com", "Password123!", It.IsAny<string>()))
        .ReturnsAsync(AuthResult.Succeed("id-token", "access-token", "refresh-token"));

      _mockAuthenticationService
        .Setup(a => a.SignInAsync(
          It.IsAny<HttpContext>(),
          It.IsAny<string>(),
          It.IsAny<ClaimsPrincipal>(),
          It.IsAny<AuthenticationProperties>()))
        .Callback<HttpContext, string, ClaimsPrincipal, AuthenticationProperties>((_, _, _, props) => capturedProperties = props)
        .Returns(Task.CompletedTask);

      // act
      await _controller.Login(model);

      // assert
      capturedProperties.Should().NotBeNull();
      capturedProperties.IsPersistent.Should().BeFalse();
    }

    private void SetupUrlHelper(bool isLocalUrl)
    {
      var mockUrlHelper = new Mock<IUrlHelper>();
      mockUrlHelper.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(isLocalUrl);
      _controller.Url = mockUrlHelper.Object;
    }

    private void SetupControllerContext(AccountController controller)
    {
      var serviceProvider = new Mock<IServiceProvider>();
      serviceProvider.Setup(sp => sp.GetService(typeof(IAuthenticationService)))
        .Returns(_mockAuthenticationService.Object);

      var httpContext = new DefaultHttpContext
      {
        RequestServices = serviceProvider.Object,
      };

      controller.ControllerContext = new ControllerContext
      {
        HttpContext = httpContext,
      };

      controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }
  }

  [TestFixture]
  public class AccountControllerSignupTests
  {
    private Mock<IAuthService> _mockAuthService;
    private AccountController _controller;

    [SetUp]
    public void SetUp()
    {
      _mockAuthService = new Mock<IAuthService>();
      _controller = AccountControllerTestHelper.CreateController(_mockAuthService);
    }

    [Test]
    public void should_return_view_with_empty_signup_view_model()
    {
      // act
      var result = _controller.Signup();

      // assert
      var viewResult = result.Should().BeOfType<ViewResult>().Subject;
      viewResult.Model.Should().BeOfType<SignupViewModel>();
    }

    [Test]
    public async Task should_redirect_to_confirm_on_successful_signup()
    {
      // arrange
      var model = new SignupViewModel { Email = "user@example.com", Username = "testuser", Password = "Password123456!" };

      _mockAuthService
        .Setup(s => s.SignUpAsync("user@example.com", "testuser", "Password123456!"))
        .ReturnsAsync(SignUpResult.Succeed());

      // act
      var result = await _controller.Signup(model);

      // assert
      var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirectResult.ActionName.Should().Be("Confirm");
      _controller.TempData["SignUpEmail"].Should().Be("user@example.com");
    }

    [Test]
    public async Task should_return_view_with_error_on_failed_signup()
    {
      // arrange
      var model = new SignupViewModel { Email = "user@example.com", Username = "testuser", Password = "weak" };

      _mockAuthService
        .Setup(s => s.SignUpAsync("user@example.com", "testuser", "weak"))
        .ReturnsAsync(SignUpResult.Fail(SignUpErrorType.InvalidPassword, "Password does not meet the required criteria."));

      // act
      var result = await _controller.Signup(model);

      // assert
      var viewResult = result.Should().BeOfType<ViewResult>().Subject;
      var vm = viewResult.Model.Should().BeOfType<SignupViewModel>().Subject;
      vm.ErrorMessage.Should().Be("Password does not meet the required criteria.");
      vm.Password.Should().BeNull();
    }
  }

  [TestFixture]
  public class AccountControllerConfirmTests
  {
    private Mock<IAuthService> _mockAuthService;
    private Mock<IBbPointsService> _mockPointsService;
    private Mock<ICognitoService> _mockCognitoService;
    private AccountController _controller;

    [SetUp]
    public void SetUp()
    {
      _mockAuthService = new Mock<IAuthService>();
      _mockPointsService = new Mock<IBbPointsService>();
      _mockCognitoService = new Mock<ICognitoService>();

      _controller = AccountControllerTestHelper.CreateController(_mockAuthService, _mockPointsService, _mockCognitoService);
    }

    [Test]
    public void should_redirect_to_signup_when_temp_data_absent()
    {
      // act
      var result = _controller.Confirm();

      // assert
      var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirectResult.ActionName.Should().Be("Signup");
    }

    [Test]
    public void should_render_confirm_view_with_email_from_temp_data()
    {
      // arrange
      _controller.TempData["SignUpEmail"] = "user@example.com";

      // act
      var result = _controller.Confirm();

      // assert
      var viewResult = result.Should().BeOfType<ViewResult>().Subject;
      var model = viewResult.Model.Should().BeOfType<ConfirmViewModel>().Subject;
      model.Email.Should().Be("user@example.com");
    }

    [Test]
    public async Task should_redirect_to_login_on_successful_confirmation()
    {
      // arrange
      var model = new ConfirmViewModel { Email = "user@example.com", Code = "123456" };

      _mockAuthService
        .Setup(s => s.ConfirmSignUpAsync("user@example.com", "123456"))
        .ReturnsAsync(ConfirmResult.Succeed());

      _mockCognitoService
        .Setup(s => s.GetUserByEmailAsync("user@example.com"))
        .ReturnsAsync(new CognitoUserInfo("user-sub-123", "testuser"));

      _mockPointsService
        .Setup(s => s.EnsureUserExistsAsync("user-sub-123", "testuser"))
        .ReturnsAsync(new User());

      // act
      var result = await _controller.Confirm(model);

      // assert
      var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirectResult.ActionName.Should().Be("Login");
    }

    [Test]
    public async Task should_return_view_with_error_on_failed_confirmation()
    {
      // arrange
      var model = new ConfirmViewModel { Email = "user@example.com", Code = "000000" };

      _mockAuthService
        .Setup(s => s.ConfirmSignUpAsync("user@example.com", "000000"))
        .ReturnsAsync(ConfirmResult.Fail(ConfirmErrorType.CodeMismatch, "Invalid confirmation code. Please check the code and try again."));

      // act
      var result = await _controller.Confirm(model);

      // assert
      var viewResult = result.Should().BeOfType<ViewResult>().Subject;
      var vm = viewResult.Model.Should().BeOfType<ConfirmViewModel>().Subject;
      vm.ErrorMessage.Should().Be("Invalid confirmation code. Please check the code and try again.");
    }

    [Test]
    public async Task should_still_redirect_to_login_when_local_user_creation_fails()
    {
      // arrange
      var model = new ConfirmViewModel { Email = "user@example.com", Code = "123456" };

      _mockAuthService
        .Setup(s => s.ConfirmSignUpAsync("user@example.com", "123456"))
        .ReturnsAsync(ConfirmResult.Succeed());

      _mockCognitoService
        .Setup(s => s.GetUserByEmailAsync("user@example.com"))
        .ThrowsAsync(new Exception("cognito error"));

      // act
      var result = await _controller.Confirm(model);

      // assert
      var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirectResult.ActionName.Should().Be("Login");
    }

    [Test]
    public async Task should_still_redirect_to_login_when_ensure_user_exists_returns_null()
    {
      // arrange
      var model = new ConfirmViewModel { Email = "user@example.com", Code = "123456" };

      _mockAuthService
        .Setup(s => s.ConfirmSignUpAsync("user@example.com", "123456"))
        .ReturnsAsync(ConfirmResult.Succeed());

      _mockCognitoService
        .Setup(s => s.GetUserByEmailAsync("user@example.com"))
        .ReturnsAsync(new CognitoUserInfo("user-sub-123", "testuser"));

      _mockPointsService
        .Setup(s => s.EnsureUserExistsAsync("user-sub-123", "testuser"))
        .ReturnsAsync((User)null);

      // act
      var result = await _controller.Confirm(model);

      // assert
      var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirectResult.ActionName.Should().Be("Login");
    }
  }

  [TestFixture]
  public class AccountControllerResendCodeTests
  {
    private Mock<IAuthService> _mockAuthService;
    private AccountController _controller;

    [SetUp]
    public void SetUp()
    {
      _mockAuthService = new Mock<IAuthService>();
      _controller = AccountControllerTestHelper.CreateController(_mockAuthService);
    }

    [Test]
    public async Task should_display_success_message_after_resend()
    {
      // arrange
      var model = new ConfirmViewModel { Email = "user@example.com" };

      _mockAuthService
        .Setup(s => s.ResendConfirmationCodeAsync("user@example.com"))
        .ReturnsAsync(ResendResult.Succeed());

      // act
      var result = await _controller.ResendCode(model);

      // assert
      var viewResult = result.Should().BeOfType<ViewResult>().Subject;
      viewResult.ViewName.Should().Be("Confirm");
      var vm = viewResult.Model.Should().BeOfType<ConfirmViewModel>().Subject;
      vm.SuccessMessage.Should().Be("A new code has been sent to your email.");
      vm.Email.Should().Be("user@example.com");
    }
  }

  [TestFixture]
  public class AccountControllerForgottenPasswordTests
  {
    private Mock<IAuthService> _mockAuthService;
    private AccountController _controller;

    [SetUp]
    public void SetUp()
    {
      _mockAuthService = new Mock<IAuthService>();
      _controller = AccountControllerTestHelper.CreateController(_mockAuthService);
    }

    [Test]
    public void should_return_view_with_empty_view_model()
    {
      // act
      var result = _controller.ForgottenPassword();

      // assert
      var viewResult = result.Should().BeOfType<ViewResult>().Subject;
      viewResult.Model.Should().BeOfType<ForgottenPasswordViewModel>();
    }

    [Test]
    public async Task should_redirect_to_reset_password_on_submit()
    {
      // arrange
      var model = new ForgottenPasswordViewModel { Email = "user@example.com" };

      _mockAuthService
        .Setup(s => s.ForgotPasswordAsync("user@example.com"))
        .ReturnsAsync(ForgotPasswordResult.Succeed());

      // act
      var result = await _controller.ForgottenPassword(model);

      // assert
      var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirectResult.ActionName.Should().Be("ResetPassword");
      redirectResult.RouteValues["email"].Should().Be("user@example.com");
    }
  }

  [TestFixture]
  public class AccountControllerResetPasswordTests
  {
    private Mock<IAuthService> _mockAuthService;
    private AccountController _controller;

    [SetUp]
    public void SetUp()
    {
      _mockAuthService = new Mock<IAuthService>();
      _controller = AccountControllerTestHelper.CreateController(_mockAuthService);
    }

    [Test]
    public void should_return_view_with_email_prepopulated()
    {
      // act
      var result = _controller.ResetPassword("user@example.com");

      // assert
      var viewResult = result.Should().BeOfType<ViewResult>().Subject;
      var model = viewResult.Model.Should().BeOfType<ResetPasswordViewModel>().Subject;
      model.Email.Should().Be("user@example.com");
    }

    [Test]
    public async Task should_redirect_to_login_on_successful_reset()
    {
      // arrange
      var model = new ResetPasswordViewModel
      {
        Email = "user@example.com",
        Code = "123456",
        NewPassword = "NewPassword123!",
        ConfirmPassword = "NewPassword123!",
      };

      _mockAuthService
        .Setup(s => s.ConfirmForgotPasswordAsync("user@example.com", "123456", "NewPassword123!"))
        .ReturnsAsync(ResetPasswordResult.Succeed());

      // act
      var result = await _controller.ResetPassword(model);

      // assert
      var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirectResult.ActionName.Should().Be("Login");
    }

    [Test]
    public async Task should_return_view_with_error_when_passwords_do_not_match()
    {
      // arrange
      var model = new ResetPasswordViewModel
      {
        Email = "user@example.com",
        Code = "123456",
        NewPassword = "Password1!",
        ConfirmPassword = "Password2!",
      };

      // act
      var result = await _controller.ResetPassword(model);

      // assert
      var viewResult = result.Should().BeOfType<ViewResult>().Subject;
      var vm = viewResult.Model.Should().BeOfType<ResetPasswordViewModel>().Subject;
      vm.ErrorMessage.Should().Be("Passwords do not match.");
    }

    [Test]
    public async Task should_return_view_with_error_on_failed_reset()
    {
      // arrange
      var model = new ResetPasswordViewModel
      {
        Email = "user@example.com",
        Code = "000000",
        NewPassword = "NewPassword123!",
        ConfirmPassword = "NewPassword123!",
      };

      _mockAuthService
        .Setup(s => s.ConfirmForgotPasswordAsync("user@example.com", "000000", "NewPassword123!"))
        .ReturnsAsync(ResetPasswordResult.Fail(ResetErrorType.CodeMismatch, "Invalid confirmation code. Please check the code and try again."));

      // act
      var result = await _controller.ResetPassword(model);

      // assert
      var viewResult = result.Should().BeOfType<ViewResult>().Subject;
      var vm = viewResult.Model.Should().BeOfType<ResetPasswordViewModel>().Subject;
      vm.ErrorMessage.Should().Be("Invalid confirmation code. Please check the code and try again.");
    }
  }

  [TestFixture]
  public class AccountControllerLogoutTests
  {
    [Test]
    public async Task should_redirect_to_login_after_signout()
    {
      // arrange
      var mockAuthenticationService = new Mock<IAuthenticationService>();
      mockAuthenticationService
        .Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
        .Returns(Task.CompletedTask);

      var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
      var mockUrlHelper = new Mock<IUrlHelper>();
      mockUrlHelperFactory
        .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
        .Returns(mockUrlHelper.Object);

      var serviceProvider = new Mock<IServiceProvider>();
      serviceProvider.Setup(sp => sp.GetService(typeof(IAuthenticationService)))
        .Returns(mockAuthenticationService.Object);
      serviceProvider.Setup(sp => sp.GetService(typeof(IUrlHelperFactory)))
        .Returns(mockUrlHelperFactory.Object);

      var httpContext = new DefaultHttpContext
      {
        RequestServices = serviceProvider.Object,
      };

      var controller = new AccountController(
        new Mock<IAuthService>().Object,
        new Mock<IBbPointsService>().Object,
        new Mock<ICognitoService>().Object,
        new Mock<ILogger<AccountController>>().Object);

      controller.ControllerContext = new ControllerContext
      {
        HttpContext = httpContext,
      };

      controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

      // act
      var result = await controller.Logout();

      // assert
      var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirectResult.ActionName.Should().Be("Login");
      mockAuthenticationService.Verify(
        a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()),
        Times.Once);
    }
  }

  internal static class AccountControllerTestHelper
  {
    internal static AccountController CreateController(
      Mock<IAuthService> mockAuthService = null,
      Mock<IBbPointsService> mockPointsService = null,
      Mock<ICognitoService> mockCognitoService = null,
      Mock<ILogger<AccountController>> mockLogger = null)
    {
      var controller = new AccountController(
        (mockAuthService ?? new Mock<IAuthService>()).Object,
        (mockPointsService ?? new Mock<IBbPointsService>()).Object,
        (mockCognitoService ?? new Mock<ICognitoService>()).Object,
        new Mock<ILogger<AccountController>>().Object);

      var httpContext = new DefaultHttpContext();
      controller.ControllerContext = new ControllerContext
      {
        HttpContext = httpContext,
      };

      controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

      return controller;
    }

    internal static AccountController CreateControllerWithAuthService(Mock<IAuthenticationService> mockAuthenticationService)
    {
      var serviceProvider = new Mock<IServiceProvider>();
      serviceProvider.Setup(sp => sp.GetService(typeof(IAuthenticationService)))
        .Returns(mockAuthenticationService.Object);

      var httpContext = new DefaultHttpContext
      {
        RequestServices = serviceProvider.Object,
      };

      var controller = new AccountController(
        new Mock<IAuthService>().Object,
        new Mock<IBbPointsService>().Object,
        new Mock<ICognitoService>().Object,
        new Mock<ILogger<AccountController>>().Object);

      controller.ControllerContext = new ControllerContext
      {
        HttpContext = httpContext,
      };

      controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

      return controller;
    }
  }
}
