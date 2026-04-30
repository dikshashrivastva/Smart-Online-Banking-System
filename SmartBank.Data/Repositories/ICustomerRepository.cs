using SmartBank.Models.Entities;

namespace SmartBank.Data.Repositories;

public interface ICustomerRepository
{
    Task<User?> GetUserAsync(int userId);
    Task UpdateUserAsync(User user);
    Task<List<Account>> GetAccountsAsync(int userId);
    Task<Account?> GetAccountByNumberAsync(string accountNumber);
    Task<bool> AccountNumberExistsAsync(string accountNumber);
    Task<Account> CreateAccountAsync(Account account);
}
