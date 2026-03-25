namespace Bejebeje.Services.Services
{
  public class ResendResult
  {
    public bool Success { get; private set; }

    public string ErrorMessage { get; private set; }

    public static ResendResult Succeed()
    {
      return new ResendResult
      {
        Success = true,
      };
    }

    public static ResendResult Fail(string errorMessage)
    {
      return new ResendResult
      {
        Success = false,
        ErrorMessage = errorMessage,
      };
    }
  }
}
