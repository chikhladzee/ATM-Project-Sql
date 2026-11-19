namespace ATM.Domain.Models.DTOs;

public class DailyFinancialStatsDto
{
  public DateTime Date { get; set; }
  public decimal TotalDeposits { get; set; }
  public decimal TotalWithdrawals { get; set; }
}
