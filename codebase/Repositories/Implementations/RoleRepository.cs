using Microsoft.EntityFrameworkCore;
using codebase.Data;
using codebase.Models.Entities;
using codebase.Repositories.Interfaces;

namespace codebase.Repositories.Implementations;

public class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _context;

    public RoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(int roleId)
    {
        return await _context.Roles.FindAsync(roleId);
    }

    public async Task<List<Role>> GetByUserIdAsync(int userId)
    {
        return await _context.Roles
            .Where(r => r.UserId == userId)
            .ToListAsync();
    }

    public async Task<Role?> GetByUserIdAndRoleNameAsync(int userId, string roleName)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.UserId == userId && r.RoleName == roleName);
    }

    public async Task<Role> CreateAsync(Role role)
    {
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task DeleteAsync(Role role)
    {
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UserHasRoleAsync(int userId, string roleName)
    {
        return await _context.Roles
            .AnyAsync(r => r.UserId == userId && r.RoleName == roleName);
    }

    public async Task<List<string>> GetUserRolesAsync(int userId)
    {
        return await _context.Roles
            .Where(r => r.UserId == userId)
            .Select(r => r.RoleName)
            .ToListAsync();
    }
}
