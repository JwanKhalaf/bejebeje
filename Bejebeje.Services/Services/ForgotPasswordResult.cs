namespace Bejebeje.Services.Services
{
  public class ForgotPasswordResult
  {
    public bool Success { get; private set; }

    public string ErrorMessage { get; private set; }

    public static ForgotPasswordResult Succeed()
    {
      return new ForgotPasswordResult
      {
        Success = true,
      };
    }

    public static ForgotPasswordResult Fail(string errorMessage)
    {
      return new ForgotPasswordResult
      {
        Success = false,
        ErrorMessage = errorMessage,
      };
    }
  }
}
