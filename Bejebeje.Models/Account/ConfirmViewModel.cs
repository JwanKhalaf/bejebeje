namespace Bejebeje.Models.Account;

public class ConfirmViewModel
{
  public string Email { get; set; }

  public string Code { get; set; }

  public string ErrorMessage { get; set; }

  public string SuccessMessage { get; set; }
}
