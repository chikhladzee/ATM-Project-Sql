using ATM.Domain.Interfaces;
using ATM.Domain.Models.Entities;
using ATM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ATM.Infrastructure.Repositories;

public class PasswordRecoveryRepository : IPasswordRecoveryRepository
{
  private readonly ATMDbContext _context;

  public PasswordRecoveryRepository(ATMDbContext context)
  {
    _context = context;
  }

  public async Task<PasswordRecovery?> GetByUserIdAsync(int userId)
  {
    return await _context.PasswordRecoveries
      .FirstOrDefaultAsync(r => r.UserId == userId);
  }

  public async Task AddAsync(PasswordRecovery recovery)
  {
    await _context.PasswordRecoveries.AddAsync(recovery);
    await _context.SaveChangesAsync();
  }

  public async Task UpdateAsync(PasswordRecovery recovery)
  {
    _context.PasswordRecoveries.Update(recovery);
    await _context.SaveChangesAsync();
  }

  public async Task DeleteAsync(PasswordRecovery recovery)
  {
    _context.PasswordRecoveries.Remove(recovery);
    await _context.SaveChangesAsync();
  }
}
