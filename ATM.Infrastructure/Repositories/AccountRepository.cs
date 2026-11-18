using ATM.Domain.Interfaces;
using ATM.Domain.Models.Entities;
using ATM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ATM.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
  private readonly ATMDbContext _context;

  public AccountRepository(ATMDbContext context)
  {
    _context = context;
  }

  public async Task<Account?> GetByIdAsync(int id)
  {
    return await _context.Accounts
      .FirstOrDefaultAsync(a => a.Id == id);
  }

  public async Task<Account?> GetByUserIdAsync(int userId)
  {
    return await _context.Accounts
      .FirstOrDefaultAsync(a => a.UserId == userId);
  }

  public async Task AddAsync(Account account)
  {
    await _context.Accounts.AddAsync(account);
    await _context.SaveChangesAsync();
  }

  public async Task UpdateAsync(Account account)
  {
    _context.Accounts.Update(account);
    await _context.SaveChangesAsync();
  }
}
