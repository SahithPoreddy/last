using codebase.Models.Entities;

namespace codebase.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(int roleId);
    Task<List<Role>> GetByUserIdAsync(int userId);
    Task<Role?> GetByUserIdAndRoleNameAsync(int userId, string roleName);
    Task<Role> CreateAsync(Role role);
    Task DeleteAsync(Role role);
    Task<bool> UserHasRoleAsync(int userId, string roleName);
    Task<List<string>> GetUserRolesAsync(int userId);
}
