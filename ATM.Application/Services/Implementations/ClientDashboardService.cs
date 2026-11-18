using ATM.Application.Services.Interfaces;
using ATM.Domain.Interfaces;
using ATM.Domain.Models.DTOs;
using ATM.Domain.Models.Entities;
using ATM.Domain.Models.Enums;

namespace ATM.Application.Services;

public class ClientDashboardService : IClientDashboardService
{
  private readonly IUserRepository _userRepository;
  private readonly IAccountRepository _accountRepository;
  private readonly ITransactionRepository _transactionRepository;

  public ClientDashboardService(
    IUserRepository userRepository,
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository)
  {
    _userRepository = userRepository;
    _accountRepository = accountRepository;
    _transactionRepository = transactionRepository;
  }

  public async Task<ClientDashboardDto> GetDashboardAsync(int userId)
  {
    User? user = await _userRepository.GetByIdAsync(userId);

    if (user == null || user.Role != UserRole.Client)
    {
      throw new UnauthorizedAccessException(
        "Only clients can access the dashboard."
      );
    }

    Account? account = await _accountRepository.GetByUserIdAsync(userId);

    if (account == null)
    {
      throw new InvalidOperationException(
        "Account not found."
      );
    }

    List<Transaction> transactions = await _transactionRepository
      .GetByAccountIdAsync(account.Id);

    decimal totalDeposits = transactions
      .Where(t => t.Type == TransactionType.Deposit)
      .Sum(t => t.Amount);

    decimal totalWithdrawals = transactions
      .Where(t => t.Type == TransactionType.Withdrawal)
      .Sum(t => t.Amount);

    List<TransactionDto> recentTransactions = transactions
      .Take(5)
      .Select(transaction => new TransactionDto
      {
        Id = transaction.Id,
        Amount = transaction.Amount,
        Type = transaction.Type,
        CreatedAt = transaction.CreatedAt
      })
      .ToList();

    return new ClientDashboardDto
    {
      Balance = account.Balance,
      TotalDeposits = totalDeposits,
      TotalWithdrawals = totalWithdrawals,
      TransactionCount = transactions.Count,
      RecentTransactions = recentTransactions
    };
  }
}
