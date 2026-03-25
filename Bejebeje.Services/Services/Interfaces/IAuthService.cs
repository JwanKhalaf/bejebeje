namespace Bejebeje.Services.Services.Interfaces;

using System.Threading.Tasks;

public interface IAuthService
{
  Task<AuthResult> AuthenticateAsync(string email, string password, string clientIp);

  Task<SignUpResult> SignUpAsync(string email, string username, string password);

  Task<ConfirmResult> ConfirmSignUpAsync(string email, string code);

  Task<ResendResult> ResendConfirmationCodeAsync(string email);

  Task<ForgotPasswordResult> ForgotPasswordAsync(string email);

  Task<ResetPasswordResult> ConfirmForgotPasswordAsync(string email, string code, string newPassword);
}
