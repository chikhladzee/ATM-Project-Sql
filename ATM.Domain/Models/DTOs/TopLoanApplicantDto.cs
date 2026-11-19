namespace ATM.Domain.Models.DTOs;

public class TopLoanApplicantDto
{
  public int UserId { get; set; }
  public string Username { get; set; } = string.Empty;
  public int RequestCount { get; set; }
}
