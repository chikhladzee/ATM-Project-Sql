using ATM.Domain.Models.Entities;

namespace ATM.Domain.Interfaces;

public interface IPasswordRecoveryRepository
{
  Task<PasswordRecovery?> GetByUserIdAsync(int userId);
  Task AddAsync(PasswordRecovery recovery);
  Task UpdateAsync(PasswordRecovery recovery);
  Task DeleteAsync(PasswordRecovery recovery);
}
