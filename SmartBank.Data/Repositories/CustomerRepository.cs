using Microsoft.EntityFrameworkCore;
using SmartBank.Data.Context;
using SmartBank.Models.Entities;

namespace SmartBank.Data.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly SmartOnlineBankingDbContext _context;

    public CustomerRepository(SmartOnlineBankingDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserAsync(int userId)
        => await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

    public async Task UpdateUserAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Account>> GetAccountsAsync(int userId)
        => await _context.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.OpenedAt)
            .ToListAsync();

    public async Task<Account?> GetAccountByNumberAsync(string accountNumber)
        => await _context.Accounts
            .AsNoTracking()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

    public async Task<bool> AccountNumberExistsAsync(string accountNumber)
        => await _context.Accounts.AnyAsync(a => a.AccountNumber == accountNumber);

    public async Task<Account> CreateAccountAsync(Account account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }
}
