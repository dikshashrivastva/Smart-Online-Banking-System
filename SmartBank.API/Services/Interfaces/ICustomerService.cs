using SmartBank.Models.DTOs.Customer;
using SmartBank.Models.DTOs.Transactions;

namespace SmartBank.API.Services.Interfaces;

public interface ICustomerService
{
    Task<CustomerProfileDto?> GetProfileAsync(int userId);
    Task<CustomerProfileDto?> UpdateProfileAsync(int userId, UpdateCustomerProfileDto request);
    Task<List<AccountDto>> GetAccountsAsync(int userId);
    Task<ToAccountLookupDto?> LookupAccountAsync(string accountNumber);
    Task<AccountDto> CreateAccountAsync(int userId, CreateAccountRequestDto request);
}
