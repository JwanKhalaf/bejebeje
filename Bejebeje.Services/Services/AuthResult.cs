namespace Bejebeje.Services.Services
{
  public enum AuthErrorType
  {
    InvalidCredentials,
    UserNotConfirmed,
    TooManyRequests,
    Unexpected,
  }

  public class AuthResult
  {
    public bool Success { get; private set; }

    public string IdToken { get; private set; }

    public string AccessToken { get; private set; }

    public string RefreshToken { get; private set; }

    public AuthErrorType? ErrorType { get; private set; }

    public string ErrorMessage { get; private set; }

    public static AuthResult Succeed(string idToken, string accessToken, string refreshToken)
    {
      return new AuthResult
      {
        Success = true,
        IdToken = idToken,
        AccessToken = accessToken,
        RefreshToken = refreshToken,
      };
    }

    public static AuthResult Fail(AuthErrorType errorType, string errorMessage)
    {
      return new AuthResult
      {
        Success = false,
        ErrorType = errorType,
        ErrorMessage = errorMessage,
      };
    }
  }
}
