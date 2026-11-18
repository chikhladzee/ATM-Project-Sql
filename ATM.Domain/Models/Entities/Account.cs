namespace ATM.Domain.Models.Entities;

public class Account
{
  public int Id { get; set; }
  public decimal Balance { get; set; } = 0;
  public int UserId { get; set; }
  public User User { get; set; } = null!;
  public List<Transaction> Transactions { get; set; } = new List<Transaction>();
}
