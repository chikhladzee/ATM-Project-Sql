using ATM.Domain.Models.Entities;

namespace ATM.Domain.Interfaces;

public interface ILoanRequestRepository
{
  Task<LoanRequest?> GetByIdAsync(int id);
  Task<List<LoanRequest>> GetAllAsync();
  Task<List<LoanRequest>> GetByUserIdAsync(int userId);
  Task AddAsync(LoanRequest loanRequest);
  Task UpdateAsync(LoanRequest loanRequest);
}
