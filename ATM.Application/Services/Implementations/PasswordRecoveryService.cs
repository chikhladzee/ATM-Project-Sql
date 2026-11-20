using System.Security.Cryptography;
using ATM.Application.Services.Interfaces;
using ATM.Domain.Interfaces;
using ATM.Domain.Models.Entities;

namespace ATM.Application.Services;

public class PasswordRecoveryService : IPasswordRecoveryService
{
  private readonly IPasswordRecoveryRepository _recoveryRepository;
  private readonly IUserRepository _userRepository;

  public PasswordRecoveryService(
    IUserRepository userRepository,
    IPasswordRecoveryRepository recoveryRepository)
  {
    _userRepository = userRepository;
    _recoveryRepository = recoveryRepository;
  }

  public async Task<bool> SendRecoveryCodeAsync(string username)
  {
    User? user = await _userRepository.GetByUsernameAsync(username);

    if (user == null)
      return false;

    string code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

    PasswordRecovery? existingRecovery =
      await _recoveryRepository.GetByUserIdAsync(user.Id);

    PasswordRecovery recovery = new PasswordRecovery
    {
      UserId = user.Id,
      CodeHash = BCrypt.Net.BCrypt.HashPassword(code),
      ExpiresAt = DateTime.UtcNow.AddMinutes(5),
      IsUsed = false,
      FailedAttempts = 0,
      CreatedAt = DateTime.UtcNow
    };

    if (existingRecovery != null) await _recoveryRepository.DeleteAsync(existingRecovery);

    await _recoveryRepository.AddAsync(recovery);

    Console.WriteLine($"Your recovery code: {code}");

    return true;
  }

  public async Task<bool> ResetPasswordAsync(
    string username,
    string code,
    string newPassword)
  {
    User? user = await _userRepository.GetByUsernameAsync(username);

    if (user == null)
      return false;

    PasswordRecovery? recovery =
      await _recoveryRepository.GetByUserIdAsync(user.Id);

    if (recovery == null)
      return false;

    if (recovery.IsUsed || recovery.ExpiresAt < DateTime.UtcNow)
    {
      await _recoveryRepository.DeleteAsync(recovery);
      return false;
    }

    if (recovery.FailedAttempts >= 3)
    {
      await _recoveryRepository.DeleteAsync(recovery);
      return false;
    }

    bool isCodeValid = BCrypt.Net.BCrypt.Verify(code, recovery.CodeHash);

    if (!isCodeValid)
    {
      recovery.FailedAttempts++;

      if (recovery.FailedAttempts >= 3)
        await _recoveryRepository.DeleteAsync(recovery);
      else
        await _recoveryRepository.UpdateAsync(recovery);

      return false;
    }

    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

    await _userRepository.UpdateAsync(user);
    await _recoveryRepository.DeleteAsync(recovery);

    return true;
  }
}
