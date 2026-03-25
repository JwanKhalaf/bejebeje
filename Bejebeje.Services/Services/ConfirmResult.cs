namespace Bejebeje.Services.Services
{
  public enum ConfirmErrorType
  {
    CodeMismatch,
    ExpiredCode,
    AliasExists,
    Unexpected,
  }

  public class ConfirmResult
  {
    public bool Success { get; private set; }

    public ConfirmErrorType? ErrorType { get; private set; }

    public string ErrorMessage { get; private set; }

    public static ConfirmResult Succeed()
    {
      return new ConfirmResult
      {
        Success = true,
      };
    }

    public static ConfirmResult Fail(ConfirmErrorType errorType, string errorMessage)
    {
      return new ConfirmResult
      {
        Success = false,
        ErrorType = errorType,
        ErrorMessage = errorMessage,
      };
    }
  }
}
