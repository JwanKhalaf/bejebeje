namespace Bejebeje.Services.Services
{
  public enum ResetErrorType
  {
    CodeMismatch,
    ExpiredCode,
    InvalidPassword,
    UserNotFound,
    Unexpected,
  }

  public class ResetPasswordResult
  {
    public bool Success { get; private set; }

    public ResetErrorType? ErrorType { get; private set; }

    public string ErrorMessage { get; private set; }

    public static ResetPasswordResult Succeed()
    {
      return new ResetPasswordResult
      {
        Success = true,
      };
    }

    public static ResetPasswordResult Fail(ResetErrorType errorType, string errorMessage)
    {
      return new ResetPasswordResult
      {
        Success = false,
        ErrorType = errorType,
        ErrorMessage = errorMessage,
      };
    }
  }
}
