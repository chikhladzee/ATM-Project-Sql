namespace ATM.Domain.Models.DTOs;

public class AdminDashboardDto
{
  public int TotalClients { get; set; }
  public int TotalTransactions { get; set; }
  public decimal TotalDeposits { get; set; }
  public decimal TotalWithdrawals { get; set; }
  public int TotalLoanRequests { get; set; }

  public int PendingLoans { get; set; }
  public int ApprovedLoans { get; set; }
  public int RejectedLoans { get; set; }
  public decimal LoanApprovalRate { get; set; }

  public List<DailyFinancialStatsDto> DailyFinancialStats { get; set; } = new List<DailyFinancialStatsDto>();
  public List<TopLoanApplicantDto> TopLoanApplicants { get; set; } = new List<TopLoanApplicantDto>();
  public List<TransactionDto> RecentTransactions { get; set; } = new List<TransactionDto>();
}
