using ATM.Domain.Models.Entities;

namespace ATM.Domain.Interfaces;

public interface ITransactionRepository
{
  Task<List<Transaction>> GetByAccountIdAsync(int accountId);
  Task AddAsync(Transaction transaction);
}
