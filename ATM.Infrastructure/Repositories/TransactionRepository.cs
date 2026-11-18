using ATM.Domain.Interfaces;
using ATM.Domain.Models.Entities;
using ATM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ATM.Infrastructure.Repositories;

public class TransactionRepository: ITransactionRepository
{
  private readonly ATMDbContext _context;

  public TransactionRepository(ATMDbContext context)
  {
    _context = context;
  }

  public async Task<List<Transaction>> GetByAccountIdAsync(int accountId)
  {
    return await _context.Transactions
      .Where(t => t.AccountId == accountId)
      .OrderByDescending(t => t.CreatedAt)
      .ToListAsync();
  }

  public async Task AddAsync(Transaction transaction)
  {
    await _context.Transactions.AddAsync(transaction);
    await _context.SaveChangesAsync();
  }
}
