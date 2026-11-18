using ATM.Domain.Models.DTOs;
using ATM.Domain.Models.Entities;

namespace ATM.Application.Services.Interfaces;

public interface ILoanService
{
  Task RequestLoanAsync(int userId, decimal amount);
  Task<List<LoanRequestDto>> GetMyRequestsAsync(int userId);
  Task<List<LoanRequestDto>> GetAllRequestsAsync(int adminId);
  Task ApproveLoanAsync(int adminId, int loanRequestId);
  Task RejectLoanAsync(int adminId, int loanRequestId);
}
