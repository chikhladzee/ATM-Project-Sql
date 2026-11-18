using ATM.Domain.Models.Enums;

namespace ATM.Domain.Models.Entities;

public class Transaction
{
  public int Id { get; set; }
  public decimal Amount { get; set; }
  public TransactionType Type { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public int AccountId { get; set; }
  public Account Account { get; set; } = null!;
}
