using ATM.Application.Services.Interfaces;
using ATM.Domain.Interfaces;
using ATM.Domain.Models.DTOs;
using ATM.Domain.Models.Entities;

namespace ATM.Application.Services;

public class TransactionService : ITransactionService
{
  private readonly IAccountRepository _accountRepository;
  private readonly ITransactionRepository _transactionRepository;

  public TransactionService(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository)
  {
    _accountRepository = accountRepository;
    _transactionRepository = transactionRepository;
  }

  public async Task<List<TransactionDto>> GetHistoryAsync(int userId)
  {
    Account? account = await _accountRepository.GetByUserIdAsync(userId);

    if (account == null)
      throw new InvalidOperationException("Account not found.");

    List<Transaction> transactions = await _transactionRepository
      .GetByAccountIdAsync(account.Id);

    return transactions.Select(transaction => new TransactionDto
    {
      Id = transaction.Id,
      Amount = transaction.Amount,
      Type = transaction.Type,
      CreatedAt = transaction.CreatedAt
    }).ToList();
  }
}
