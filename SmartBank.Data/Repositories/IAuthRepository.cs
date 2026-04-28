using SmartBank.Models.Entities;

namespace SmartBank.Data.Repositories;

public interface IAuthRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int userId);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> PhoneExistsAsync(string phone);
    Task<bool> NationalIdExistsAsync(string nationalId);
    Task<User> CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task<int> GetCustomerRoleIdAsync();
}
