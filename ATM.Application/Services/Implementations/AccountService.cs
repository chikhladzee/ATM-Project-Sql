using ATM.Application.Services.Interfaces;
using ATM.Domain.Interfaces;
using ATM.Domain.Models.Entities;
using ATM.Domain.Models.Enums;

namespace ATM.Application.Services;

public class AccountService : IAccountService
{
  private readonly IAccountRepository _accountRepository;
  private readonly ITransactionRepository _transactionRepository;
  private readonly IUserRepository _userRepository;

  public AccountService(
    IUserRepository userRepository,
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository)
  {
    _userRepository = userRepository;
    _accountRepository = accountRepository;
    _transactionRepository = transactionRepository;
  }

  public async Task<decimal> GetBalanceAsync(int userId)
  {
    Account account = await GetClientAccountAsync(userId);

    return account.Balance;
  }

  public async Task DepositAsync(int userId, decimal amount)
  {
    if (amount <= 0)
      throw new ArgumentException("Amount must be greater than zero.");

    Account? account = await GetClientAccountAsync(userId);

    account.Balance += amount;

    await _accountRepository.UpdateAsync(account);

    Transaction transaction = new Transaction
    {
      Amount = amount,
      Type = TransactionType.Deposit,
      AccountId = account.Id
    };

    await _transactionRepository.AddAsync(transaction);
  }

  public async Task WithdrawAsync(int userId, decimal amount)
  {
    if (amount <= 0)
      throw new ArgumentException("Amount must be greater than zero.");

    Account account = await GetClientAccountAsync(userId);

    if (account.Balance < amount)
      throw new InvalidOperationException("Insufficient balance.");

    account.Balance -= amount;

    await _accountRepository.UpdateAsync(account);

    Transaction transaction = new Transaction
    {
      Amount = amount,
      Type = TransactionType.Withdrawal,
      AccountId = account.Id
    };

    await _transactionRepository.AddAsync(transaction);
  }

  private async Task<Account> GetClientAccountAsync(int userId)
  {
    User? user = await _userRepository.GetByIdAsync(userId);

    if (user == null)
      throw new InvalidOperationException("User not found.");

    if (user.Role != UserRole.Client)
      throw new UnauthorizedAccessException(
        "Only clients can perform financial operations."
      );

    Account? account = await _accountRepository.GetByUserIdAsync(userId);

    if (account == null)
      throw new InvalidOperationException("Account not found.");

    return account;
  }
}
