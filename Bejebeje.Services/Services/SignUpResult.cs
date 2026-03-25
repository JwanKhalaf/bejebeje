namespace Bejebeje.Services.Services
{
  public enum SignUpErrorType
  {
    UsernameExists,
    InvalidPassword,
    InvalidParameter,
    Unexpected,
  }

  public class SignUpResult
  {
    public bool Success { get; private set; }

    public SignUpErrorType? ErrorType { get; private set; }

    public string ErrorMessage { get; private set; }

    public static SignUpResult Succeed()
    {
      return new SignUpResult
      {
        Success = true,
      };
    }

    public static SignUpResult Fail(SignUpErrorType errorType, string errorMessage)
    {
      return new SignUpResult
      {
        Success = false,
        ErrorType = errorType,
        ErrorMessage = errorMessage,
      };
    }
  }
}
