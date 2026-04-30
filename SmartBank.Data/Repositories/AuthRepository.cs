using Microsoft.EntityFrameworkCore;
using SmartBank.Data.Context;
using SmartBank.Models.Entities;

namespace SmartBank.Data.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly SmartOnlineBankingDbContext _context;

    public AuthRepository(SmartOnlineBankingDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<User?> GetByIdAsync(int userId)
        => await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

    public async Task<bool> EmailExistsAsync(string email)
        => await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<bool> PhoneExistsAsync(string phone)
        => await _context.Users.AnyAsync(u => u.PhoneNumber == phone);

    public async Task<bool> NationalIdExistsAsync(string nationalId)
        => await _context.Users.AnyAsync(u => u.NationalId == nationalId);

    public async Task<User> CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateUserAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCustomerRoleIdAsync()
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer")
                   ?? throw new InvalidOperationException("Customer role not seeded.");
        return role.RoleId;
    }
}
