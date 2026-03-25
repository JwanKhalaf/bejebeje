namespace Bejebeje.Models.Account;

public class LoginViewModel
{
  public string Email { get; set; }

  public string Password { get; set; }

  public bool RememberMe { get; set; }

  public string ReturnUrl { get; set; }

  public string ErrorMessage { get; set; }
}
