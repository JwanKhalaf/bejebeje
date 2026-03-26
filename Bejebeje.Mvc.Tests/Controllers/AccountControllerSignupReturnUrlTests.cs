namespace Bejebeje.Mvc.Tests.Controllers
{
  using System.Threading.Tasks;
  using Bejebeje.Models.Account;
  using Bejebeje.Services.Services;
  using FluentAssertions;
  using Microsoft.AspNetCore.Mvc;
  using Moq;
  using NUnit.Framework;
  using Services.Services.Interfaces;

  [TestFixture]
  public class AccountControllerSignupReturnUrlTests
  {
    [Test]
    public void should_accept_return_url_on_signup_get()
    {
      // arrange
      var controller = AccountControllerTestHelper.CreateController();

      // act
      var result = controller.Signup("/artists?entry_point=hero_add_lyric");

      // assert
      var viewResult = result.Should().BeOfType<ViewResult>().Subject;
      var model = viewResult.Model.Should().BeOfType<SignupViewModel>().Subject;
      model.ReturnUrl.Should().Be("/artists?entry_point=hero_add_lyric");
    }

    [Test]
    public async Task should_preserve_return_url_in_tempdata_after_successful_signup()
    {
      // arrange
      var mockAuthService = new Mock<IAuthService>();
      mockAuthService
        .Setup(x => x.SignUpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(SignUpResult.Succeed());

      var controller = AccountControllerTestHelper.CreateController(mockAuthService);

      var model = new SignupViewModel
      {
        Email = "test@example.com",
        Username = "testuser",
        Password = "TestPassword123!",
        ReturnUrl = "/artists/new/selector?entry_point=hero_add_artist",
      };

      // act
      var result = await controller.Signup(model);

      // assert
      controller.TempData["SignUpReturnUrl"].Should().Be("/artists/new/selector?entry_point=hero_add_artist");
    }

    [Test]
    public void should_pass_return_url_from_tempdata_to_confirm_view_model()
    {
      // arrange
      var controller = AccountControllerTestHelper.CreateController();
      controller.TempData["SignUpEmail"] = "test@example.com";
      controller.TempData["SignUpReturnUrl"] = "/artists";

      // act
      var result = controller.Confirm();

      // assert
      var viewResult = result.Should().BeOfType<ViewResult>().Subject;
      var model = viewResult.Model.Should().BeOfType<ConfirmViewModel>().Subject;
      model.ReturnUrl.Should().Be("/artists");
    }

    [Test]
    public async Task should_redirect_to_login_with_return_url_after_confirmation()
    {
      // arrange
      var mockAuthService = new Mock<IAuthService>();
      mockAuthService
        .Setup(x => x.ConfirmSignUpAsync(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(ConfirmResult.Succeed());

      var mockCognitoService = new Mock<ICognitoService>();
      mockCognitoService
        .Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
        .ReturnsAsync((CognitoUserInfo)null);

      var controller = AccountControllerTestHelper.CreateController(
        mockAuthService,
        mockCognitoService: mockCognitoService);

      var model = new ConfirmViewModel
      {
        Email = "test@example.com",
        Code = "123456",
        ReturnUrl = "/artists?entry_point=hero_add_lyric",
      };

      // act
      var result = await controller.Confirm(model);

      // assert
      var redirectResult = result.Should().BeOfType<RedirectResult>().Subject;
      redirectResult.Url.Should().Contain("/login");
      redirectResult.Url.Should().Contain("returnUrl");
    }
  }
}
