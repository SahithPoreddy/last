using codebase.Models.Entities;

namespace codebase.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int userId);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> EmailExistsAsync(string email);
}
