using ATM.Domain.Models.Entities;

namespace ATM.Domain.Interfaces;

public interface IAccountRepository
{
  Task<Account?> GetByIdAsync(int id);
  Task<Account?> GetByUserIdAsync(int userId);
  Task AddAsync(Account account);
  Task UpdateAsync(Account account);
}
