using ATM.Domain.Models.Enums;

namespace ATM.Domain.Models.DTOs;

public class UserDto
{
  public int Id { get; set; }
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;
  public string Username { get; set; } = string.Empty;
  public UserRole Role { get; set; }
  public DateTime CreatedAt { get; set; }
}
