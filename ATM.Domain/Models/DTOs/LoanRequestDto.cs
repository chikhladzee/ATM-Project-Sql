using ATM.Domain.Models.Enums;

namespace ATM.Domain.Models.DTOs;

public class LoanRequestDto
{
  public int Id { get; set; }
  public decimal Amount { get; set; }
  public LoanStatus Status { get; set; }
  public DateTime CreatedAt { get; set; }
  public int UserId { get; set; }
}
