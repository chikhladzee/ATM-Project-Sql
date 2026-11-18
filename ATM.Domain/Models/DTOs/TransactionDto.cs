using ATM.Domain.Models.Enums;

namespace ATM.Domain.Models.DTOs;

public class TransactionDto
{
  public int Id { get; set; }
  public decimal Amount { get; set; }
  public TransactionType Type { get; set; }
  public DateTime CreatedAt { get; set; }
}
