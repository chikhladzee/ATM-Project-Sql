using ATM.Domain.Interfaces;
using ATM.Domain.Models.Entities;
using ATM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ATM.Infrastructure.Repositories;

public class LoanRequestRepository : ILoanRequestRepository
{
  private readonly ATMDbContext _context;

  public LoanRequestRepository(ATMDbContext context)
  {
    _context = context;
  }

  public async Task<LoanRequest?> GetByIdAsync(int id)
  {
    return await _context.LoanRequests
      .FirstOrDefaultAsync(l => l.Id == id);
  }

  public async Task<List<LoanRequest>> GetAllAsync()
  {
    return await _context.LoanRequests
      .ToListAsync();
  }

  public async Task<List<LoanRequest>> GetByUserIdAsync(int userId)
  {
    return await _context.LoanRequests
      .Where(l => l.UserId == userId)
      .ToListAsync();
  }

  public async Task AddAsync(LoanRequest loanRequest)
  {
    await _context.LoanRequests.AddAsync(loanRequest);
    await _context.SaveChangesAsync();
  }

  public async Task UpdateAsync(LoanRequest loanRequest)
  {
    _context.LoanRequests.Update(loanRequest);
    await _context.SaveChangesAsync();
  }
}
