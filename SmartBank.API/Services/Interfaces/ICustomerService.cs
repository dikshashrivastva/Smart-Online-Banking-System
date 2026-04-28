using SmartBank.Models.DTOs.Customer;

namespace SmartBank.API.Services.Interfaces;

public interface ICustomerService
{
    Task<CustomerProfileDto?> GetProfileAsync(int userId);
    Task<CustomerProfileDto?> UpdateProfileAsync(int userId, UpdateCustomerProfileDto request);
    Task<List<AccountDto>> GetAccountsAsync(int userId);
    Task<AccountDto> CreateAccountAsync(int userId, CreateAccountRequestDto request);
}
