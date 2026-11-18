using ATM.Application.Services.Interfaces;
using ATM.Domain.Interfaces;
using ATM.Domain.Models.DTOs;
using ATM.Domain.Models.Entities;
using ATM.Domain.Models.Enums;

namespace ATM.Application.Services;

public class UserService : IUserService
{
  private readonly IUserRepository _userRepository;

  public UserService(IUserRepository userRepository)
  {
    _userRepository = userRepository;
  }

  public async Task<User> RegisterAsync(
    string firstName,
    string lastName,
    string username,
    string password)
  {
    ValidateRegistration(
      firstName,
      lastName,
      username,
      password
    );

    firstName = firstName.Trim();
    lastName = lastName.Trim();

    username = username.Trim();

    User? existingUser =
      await _userRepository.GetByUsernameAsync(username);

    if (existingUser != null)
    {
      throw new InvalidOperationException(
        "Username already exists."
      );
    }

    User user = new User
    {
      FirstName = firstName,
      LastName = lastName,
      Username = username,
      PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
      Role = UserRole.Client,
      Account = new Account
      {
        Balance = 0
      }
    };

    await _userRepository.AddAsync(user);

    return user;
  }

  public async Task<User?> LoginAsync(
    string username,
    string password)
  {
    if (string.IsNullOrWhiteSpace(username) ||
        string.IsNullOrWhiteSpace(password))
    {
      return null;
    }

    User? user = await _userRepository.GetByUsernameAsync(
      username.Trim()
    );

    if (user == null)
    {
      return null;
    }

    bool isPasswordValid =
      BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

    if (!isPasswordValid)
    {
      return null;
    }

    return user;
  }

  public async Task EnsureAdminExistsAsync(string username, string password)
  {
    User? existingUser = await _userRepository.GetByUsernameAsync(username);

    if (existingUser != null)
    {
      if (existingUser.Role != UserRole.Admin)
      {
        throw new InvalidOperationException(
          "This username already belongs to a Client."
        );
      }

      return;
    }

    User admin = new User
    {
      FirstName = "System",
      LastName = "Administrator",
      Username = username,
      PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
      Role = UserRole.Admin
    };

    await _userRepository.AddAsync(admin);
  }

  public async Task<List<UserDto>> GetAllUsersAsync(int adminId)
  {
    User? admin = await _userRepository.GetByIdAsync(adminId);

    if (admin == null || admin.Role != UserRole.Admin)
    {
      throw new UnauthorizedAccessException(
        "Only administrators can view users."
      );
    }

    List<User> users = await _userRepository.GetAllAsync();

    return users.Select(user => new UserDto
    {
      Id = user.Id,
      FirstName = user.FirstName,
      LastName = user.LastName,
      Username = user.Username,
      Role = user.Role,
      CreatedAt = user.CreatedAt
    }).ToList();
  }

  private void ValidateRegistration(
    string firstName,
    string lastName,
    string username,
    string password)
  {
    if (string.IsNullOrWhiteSpace(firstName))
      throw new ArgumentException("First name is required.");

    if (firstName.Trim().Length < 2)
      throw new ArgumentException(
        "First name must contain at least 2 characters."
      );

    if (string.IsNullOrWhiteSpace(lastName))
      throw new ArgumentException("Last name is required.");

    if (lastName.Trim().Length < 2)
      throw new ArgumentException(
        "Last name must contain at least 2 characters."
      );

    if (string.IsNullOrWhiteSpace(username))
      throw new ArgumentException("Username is required.");

    username = username.Trim();

    if (username.Length < 4 || username.Length > 20)
      throw new ArgumentException(
        "Username must be between 4 and 20 characters."
      );

    if (string.IsNullOrWhiteSpace(password))
      throw new ArgumentException("Password is required.");

    if (password.Length < 8)
      throw new ArgumentException(
        "Password must contain at least 8 characters."
      );

    if (!password.Any(char.IsDigit))
      throw new ArgumentException(
        "Password must contain at least one digit."
      );

    if (!password.Any(char.IsUpper))
      throw new ArgumentException(
        "Password must contain at least one uppercase letter."
      );
  }
}
