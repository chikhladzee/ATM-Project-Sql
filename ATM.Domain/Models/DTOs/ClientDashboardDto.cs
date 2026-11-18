using ATM.Domain.Models.Entities;

namespace ATM.Domain.Models.DTOs;

public class ClientDashboardDto
{
  public decimal Balance { get; set; }
  public decimal TotalDeposits { get; set; }
  public decimal TotalWithdrawals { get; set; }
  public int TransactionCount { get; set; }
  public List<TransactionDto> RecentTransactions { get; set; } = new List<TransactionDto>();
}
