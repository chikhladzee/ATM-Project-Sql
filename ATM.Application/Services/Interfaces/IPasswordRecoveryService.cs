namespace ATM.Application.Services.Interfaces;

public interface IPasswordRecoveryService
{
  Task<bool> SendRecoveryCodeAsync(string username);

  Task<bool> ResetPasswordAsync(
    string username,
    string code,
    string newPassword);
}
