namespace Bejebeje.Mvc.Controllers
{
  using System;
  using System.Linq;
  using System.Net;
  using System.Security.Claims;
  using System.Threading.Tasks;
  using Bejebeje.Models.Account;
  using Bejebeje.Services.Services;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.AspNetCore.Authentication;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.RateLimiting;
  using Microsoft.Extensions.Logging;

  [AllowAnonymous]
  public class AccountController : Controller
  {
    private readonly IAuthService _authService;
    private readonly IBbPointsService _bbPointsService;
    private readonly ICognitoService _cognitoService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
      IAuthService authService,
      IBbPointsService bbPointsService,
      ICognitoService cognitoService,
      ILogger<AccountController> logger)
    {
      _authService = authService;
      _bbPointsService = bbPointsService;
      _cognitoService = cognitoService;
      _logger = logger;
    }

    [HttpGet("/login")]
    public IActionResult Login(string returnUrl = null)
    {
      var viewModel = new LoginViewModel
      {
        ReturnUrl = returnUrl,
      };

      // show success message if user just confirmed their email
      if (TempData["ConfirmationSuccess"] != null)
      {
        ViewData["SuccessMessage"] = "Your email has been confirmed. You can now log in.";
      }

      return View(viewModel);
    }

    [HttpPost("/login")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth-login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      string clientIp = GetClientIp();

      var result = await _authService.AuthenticateAsync(model.Email, model.Password, clientIp);

      if (!result.Success)
      {
        if (result.ErrorType == AuthErrorType.Unexpected)
        {
          _logger.LogError("unexpected authentication error for {Email}", model.Email);
        }
        else
        {
          _logger.LogWarning("authentication failed for {Email}: {ErrorType}", model.Email, result.ErrorType);
        }

        model.ErrorMessage = result.ErrorMessage;
        model.Password = null;
        return View(model);
      }

      // create claims identity with temporary token claims
      var identity = new ClaimsIdentity("Cookies");
      identity.AddClaim(new Claim("IdToken", result.IdToken));
      identity.AddClaim(new Claim("AccessToken", result.AccessToken));
      identity.AddClaim(new Claim("RefreshToken", result.RefreshToken));

      var principal = new ClaimsPrincipal(identity);

      var authProperties = new AuthenticationProperties
      {
        IsPersistent = model.RememberMe,
      };

      if (model.RememberMe)
      {
        authProperties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30);
      }

      await HttpContext.SignInAsync("Cookies", principal, authProperties);

      _logger.LogInformation("user logged in successfully: {Email}", model.Email);

      if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
      {
        return Redirect(model.ReturnUrl);
      }

      return Redirect("/");
    }

    [HttpGet("/signup")]
    public IActionResult Signup()
    {
      return View(new SignupViewModel());
    }

    [HttpPost("/signup")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth-signup")]
    public async Task<IActionResult> Signup(SignupViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      var result = await _authService.SignUpAsync(model.Email, model.Username, model.Password);

      if (!result.Success)
      {
        model.ErrorMessage = result.ErrorMessage;
        model.Password = null;
        return View(model);
      }

      TempData["SignUpEmail"] = model.Email;
      return RedirectToAction(nameof(Confirm));
    }

    [HttpGet("/signup/confirm")]
    public IActionResult Confirm()
    {
      string email = TempData["SignUpEmail"] as string;

      if (string.IsNullOrEmpty(email))
      {
        return RedirectToAction(nameof(Signup));
      }

      var viewModel = new ConfirmViewModel
      {
        Email = email,
      };

      return View(viewModel);
    }

    [HttpPost("/signup/confirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(ConfirmViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      var result = await _authService.ConfirmSignUpAsync(model.Email, model.Code);

      if (!result.Success)
      {
        model.ErrorMessage = result.ErrorMessage;
        return View(model);
      }

      // attempt local user creation after successful email verification
      try
      {
        var cognitoUser = await _cognitoService.GetUserByEmailAsync(model.Email);

        if (cognitoUser != null)
        {
          var user = await _bbPointsService.EnsureUserExistsAsync(cognitoUser.Sub, cognitoUser.PreferredUsername);

          if (user == null)
          {
            _logger.LogWarning("EnsureUserExistsAsync returned null for {Email} after confirmation (possible duplicate username)", model.Email);
          }
          else
          {
            _logger.LogInformation("local user record created for {Email} after email confirmation", model.Email);
          }
        }
        else
        {
          _logger.LogWarning("could not find cognito user by email {Email} after confirmation", model.Email);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "failed to create local user record for {Email} after confirmation, OnSigningIn will retry", model.Email);
      }

      TempData["ConfirmationSuccess"] = true;
      return RedirectToAction(nameof(Login));
    }

    [HttpPost("/signup/confirm/resend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendCode(ConfirmViewModel model)
    {
      await _authService.ResendConfirmationCodeAsync(model.Email);

      model.SuccessMessage = "A new code has been sent to your email.";
      model.Code = null;
      return View("Confirm", model);
    }

    [HttpGet("/forgotten-password")]
    public IActionResult ForgottenPassword()
    {
      return View(new ForgottenPasswordViewModel());
    }

    [HttpPost("/forgotten-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgottenPassword(ForgottenPasswordViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      await _authService.ForgotPasswordAsync(model.Email);

      // always redirect regardless of whether user exists (no info disclosure)
      return RedirectToAction(nameof(ResetPassword), new { email = model.Email });
    }

    [HttpGet("/reset-password")]
    public IActionResult ResetPassword(string email = null)
    {
      var viewModel = new ResetPasswordViewModel
      {
        Email = email,
      };

      return View(viewModel);
    }

    [HttpPost("/reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      if (model.NewPassword != model.ConfirmPassword)
      {
        model.ErrorMessage = "Passwords do not match.";
        model.NewPassword = null;
        model.ConfirmPassword = null;
        return View(model);
      }

      var result = await _authService.ConfirmForgotPasswordAsync(model.Email, model.Code, model.NewPassword);

      if (!result.Success)
      {
        model.ErrorMessage = result.ErrorMessage;
        model.NewPassword = null;
        model.ConfirmPassword = null;
        return View(model);
      }

      return RedirectToAction(nameof(Login));
    }

    [HttpGet("/logout")]
    public async Task<IActionResult> Logout()
    {
      await HttpContext.SignOutAsync();

      _logger.LogInformation("user logged out");

      return RedirectToAction(nameof(Login));
    }

    private string GetClientIp()
    {
      // check x-forwarded-for first (behind proxy)
      string forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

      if (!string.IsNullOrEmpty(forwardedFor))
      {
        // take the first ip in the chain
        string firstIp = forwardedFor.Split(',')[0].Trim();

        if (IPAddress.TryParse(firstIp, out _))
        {
          return firstIp;
        }
      }

      return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
  }
}
