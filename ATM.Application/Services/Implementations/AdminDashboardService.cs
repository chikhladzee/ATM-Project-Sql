using ATM.Application.Services.Interfaces;
using ATM.Domain.Interfaces;
using ATM.Domain.Models.DTOs;
using ATM.Domain.Models.Entities;
using ATM.Domain.Models.Enums;

namespace ATM.Application.Services;

public class AdminDashboardService : IAdminDashboardService
{
  private readonly IAccountRepository _accountRepository;
  private readonly ILoanRequestRepository _loanRequestRepository;
  private readonly ITransactionRepository _transactionRepository;
  private readonly IUserRepository _userRepository;

  public AdminDashboardService(
    IUserRepository userRepository,
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    ILoanRequestRepository loanRequestRepository)
  {
    _userRepository = userRepository;
    _accountRepository = accountRepository;
    _transactionRepository = transactionRepository;
    _loanRequestRepository = loanRequestRepository;
  }

  public async Task<AdminDashboardDto> GetDashboardAsync()
  {
    List<User> users = await _userRepository.GetAllAsync();
    List<LoanRequest> loans = await _loanRequestRepository.GetAllAsync();

    List<Account> accounts = new List<Account>();
    List<Transaction> transactions = new List<Transaction>();

    foreach (User user in users.Where(u => u.Role == UserRole.Client))
    {
      Account? account = await _accountRepository.GetByUserIdAsync(user.Id);

      if (account is null)
        continue;

      accounts.Add(account);

      List<Transaction> accountTransactions =
        await _transactionRepository.GetByAccountIdAsync(account.Id);

      transactions.AddRange(accountTransactions);
    }

    int totalClients = users.Count(u => u.Role == UserRole.Client);

    decimal totalDeposits = transactions
      .Where(t => t.Type == TransactionType.Deposit)
      .Sum(t => t.Amount);

    decimal totalWithdrawals = transactions
      .Where(t => t.Type == TransactionType.Withdrawal)
      .Sum(t => t.Amount);

    int pendingLoans = loans.Count(l => l.Status == LoanStatus.Pending);
    int approvedLoans = loans.Count(l => l.Status == LoanStatus.Approved);
    int rejectedLoans = loans.Count(l => l.Status == LoanStatus.Rejected);

    decimal loanApprovalRate = loans.Count == 0
      ? 0
      : (decimal)approvedLoans / loans.Count * 100;

    DateTime today = DateTime.Now.Date;
    List<DailyFinancialStatsDto> dailyFinancialStats = new List<DailyFinancialStatsDto>();

    for (int i = 9; i >= 0; i--)
    {
      DateTime date = today.AddDays(-i);

      List<Transaction> dayTransactions = transactions
        .Where(t => t.CreatedAt.ToLocalTime().Date == date)
        .ToList();

      dailyFinancialStats.Add(new DailyFinancialStatsDto
      {
        Date = date,

        TotalDeposits = dayTransactions
          .Where(t => t.Type == TransactionType.Deposit)
          .Sum(t => t.Amount),

        TotalWithdrawals = dayTransactions
          .Where(t => t.Type == TransactionType.Withdrawal)
          .Sum(t => t.Amount)
      });
    }

    List<TopLoanApplicantDto> topLoanApplicants = loans
      .GroupBy(l => l.UserId)
      .Select(group =>
      {
        User? user = users.FirstOrDefault(u => u.Id == group.Key);

        return new TopLoanApplicantDto
        {
          UserId = group.Key,
          Username = user?.Username ?? "Unknown",
          RequestCount = group.Count()
        };
      })
      .OrderByDescending(u => u.RequestCount)
      .Take(5)
      .ToList();

    List<TransactionDto> recentTransactions = transactions
      .OrderByDescending(t => t.CreatedAt)
      .Take(10)
      .Select(t => new TransactionDto
      {
        Id = t.Id,
        Amount = t.Amount,
        Type = t.Type,
        CreatedAt = t.CreatedAt
      })
      .ToList();

    return new AdminDashboardDto
    {
      TotalClients = totalClients,
      TotalTransactions = transactions.Count,
      TotalDeposits = totalDeposits,
      TotalWithdrawals = totalWithdrawals,
      TotalLoanRequests = loans.Count,

      PendingLoans = pendingLoans,
      ApprovedLoans = approvedLoans,
      RejectedLoans = rejectedLoans,
      LoanApprovalRate = loanApprovalRate,

      DailyFinancialStats = dailyFinancialStats,
      TopLoanApplicants = topLoanApplicants,
      RecentTransactions = recentTransactions
    };
  }
}
