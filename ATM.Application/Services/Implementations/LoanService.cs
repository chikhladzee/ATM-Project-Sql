using ATM.Application.Services.Interfaces;
using ATM.Domain.Interfaces;
using ATM.Domain.Models.DTOs;
using ATM.Domain.Models.Entities;
using ATM.Domain.Models.Enums;

namespace ATM.Application.Services;

public class LoanService : ILoanService
{
  private readonly ILoanRequestRepository _loanRequestRepository;
  private readonly IAccountRepository _accountRepository;
  private readonly ITransactionRepository _transactionRepository;
  private readonly IUserRepository _userRepository;

  public LoanService(
    ILoanRequestRepository loanRequestRepository,
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    IUserRepository userRepository)
  {
    _loanRequestRepository = loanRequestRepository;
    _accountRepository = accountRepository;
    _transactionRepository = transactionRepository;
    _userRepository = userRepository;
  }

  public async Task RequestLoanAsync(int userId, decimal amount)
  {
    if (amount <= 0)
      throw new ArgumentException("Loan amount must be greater than zero.");

    User user = await GetUserAsync(userId);

    if (user.Role != UserRole.Client)
      throw new UnauthorizedAccessException("Only clients can request loans.");

    Account? account = await _accountRepository.GetByUserIdAsync(userId);

    if (account == null)
      throw new InvalidOperationException("Account not found.");

    LoanRequest loanRequest = new LoanRequest
    {
      Amount = amount,
      UserId = userId,
      Status = LoanStatus.Pending
    };

    await _loanRequestRepository.AddAsync(loanRequest);
  }

  public async Task<List<LoanRequestDto>> GetMyRequestsAsync(int userId)
  {
    User user = await GetUserAsync(userId);

    if (user.Role != UserRole.Client)
      throw new UnauthorizedAccessException("Only clients can view their requests.");

    List<LoanRequest> requests = await _loanRequestRepository
      .GetByUserIdAsync(userId);

    return requests.Select(request => new LoanRequestDto
    {
      Id = request.Id,
      Amount = request.Amount,
      Status = request.Status,
      CreatedAt = request.CreatedAt,
      UserId = request.UserId
    }).ToList();
  }

  public async Task<List<LoanRequestDto>> GetAllRequestsAsync(int adminId)
  {
    await GetAdminAsync(adminId);

    List<LoanRequest> requests = await _loanRequestRepository.GetAllAsync();

    return requests.Select(request => new LoanRequestDto
    {
      Id = request.Id,
      Amount = request.Amount,
      Status = request.Status,
      CreatedAt = request.CreatedAt,
      UserId = request.UserId
    }).ToList();
  }

  public async Task ApproveLoanAsync(int adminId, int loanRequestId)
  {
    await GetAdminAsync(adminId);

    LoanRequest? loanRequest = await _loanRequestRepository.GetByIdAsync(loanRequestId);

    if (loanRequest == null)
      throw new InvalidOperationException("Loan request not found.");

    if (loanRequest.Status != LoanStatus.Pending)
      throw new InvalidOperationException("Loan request has already been processed.");

    Account? account = await _accountRepository.GetByUserIdAsync(loanRequest.UserId);

    if (account == null)
      throw new InvalidOperationException("Account not found.");

    loanRequest.Status = LoanStatus.Approved;
    account.Balance += loanRequest.Amount;

    Transaction transaction = new Transaction
    {
      Amount = loanRequest.Amount,
      Type = TransactionType.Loan,
      AccountId = account.Id
    };

    await _loanRequestRepository.UpdateAsync(loanRequest);
    await _accountRepository.UpdateAsync(account);
    await _transactionRepository.AddAsync(transaction);
  }

  public async Task RejectLoanAsync(int adminId, int loanRequestId)
  {
    await GetAdminAsync(adminId);

    LoanRequest? loanRequest = await _loanRequestRepository.GetByIdAsync(loanRequestId);

    if (loanRequest == null)
      throw new InvalidOperationException("Loan request not found.");

    if (loanRequest.Status != LoanStatus.Pending)
      throw new InvalidOperationException("Loan request has already been processed.");

    loanRequest.Status = LoanStatus.Rejected;

    await _loanRequestRepository.UpdateAsync(loanRequest);
  }

  private async Task<User> GetUserAsync(int userId)
  {
    User? user = await _userRepository.GetByIdAsync(userId);

    if (user == null)
      throw new InvalidOperationException("User not found.");

    return user;
  }

  private async Task<User> GetAdminAsync(int userId)
  {
    User user = await GetUserAsync(userId);

    if (user.Role != UserRole.Admin)
      throw new UnauthorizedAccessException("Admin access required.");

    return user;
  }
}
