using ATM.Domain.Models.Enums;

namespace ATM.Domain.Models.Entities;

public class User
{
  public int Id { get; set; }
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;
  public string Username { get; set; } = string.Empty;
  public string PasswordHash { get; set; } = string.Empty;
  public UserRole Role { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public Account? Account { get; set; }
  public List<LoanRequest> LoanRequests { get; set; } = new List<LoanRequest>();
  public PasswordRecovery? PasswordRecovery { get; set; }
}
