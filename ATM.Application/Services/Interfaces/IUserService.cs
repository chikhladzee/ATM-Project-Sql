using ATM.Domain.Models.DTOs;
using ATM.Domain.Models.Entities;

namespace ATM.Application.Services.Interfaces;

public interface IUserService
{
  Task<User> RegisterAsync(
    string firstName,
    string lastName,
    string username,
    string password
  );

  Task<User?> LoginAsync(
    string username,
    string password
  );

  Task EnsureAdminExistsAsync(string username, string password);
  Task<List<UserDto>> GetAllUsersAsync(int adminId);

  Task<bool> ChangePasswordAsync(
    int userId,
    string currentPassword,
    string newPassword
  );
}
