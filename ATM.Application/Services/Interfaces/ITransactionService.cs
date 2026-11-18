using ATM.Domain.Models.DTOs;
using ATM.Domain.Models.Entities;

namespace ATM.Application.Services.Interfaces;

public interface ITransactionService
{
  Task<List<TransactionDto>> GetHistoryAsync(int userId);
}
