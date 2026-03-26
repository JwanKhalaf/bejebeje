namespace Bejebeje.Models.Account;

public class SignupViewModel
{
  public string Email { get; set; }

  public string Username { get; set; }

  public string Password { get; set; }

  public string ErrorMessage { get; set; }

  public string ReturnUrl { get; set; }
}
