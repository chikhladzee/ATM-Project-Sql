using ATM.Domain.Models.Entities;

namespace ATM.Domain.Interfaces;

public interface IUserRepository
{
  Task<User?> GetByIdAsync(int id);
  Task<User?> GetByUsernameAsync(string username);
  Task<List<User>> GetAllAsync();
  Task AddAsync(User user);
  Task UpdateAsync(User user);
}
