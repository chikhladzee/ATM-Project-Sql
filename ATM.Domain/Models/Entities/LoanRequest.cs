using ATM.Domain.Models.Enums;

namespace ATM.Domain.Models.Entities;

public class LoanRequest
{
  public int Id { get; set; }
  public decimal Amount { get; set; }
  public LoanStatus Status { get; set; } = LoanStatus.Pending;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public int UserId { get; set; }
  public User User { get; set; } = null!;
}
