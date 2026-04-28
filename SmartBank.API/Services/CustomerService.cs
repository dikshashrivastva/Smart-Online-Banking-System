using SmartBank.API.Services.Interfaces;
using SmartBank.Data.Repositories;
using SmartBank.Models.DTOs.Customer;
using SmartBank.Models.Entities;

namespace SmartBank.API.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repo;

    public CustomerService(ICustomerRepository repo)
    {
        _repo = repo;
    }

    public async Task<CustomerProfileDto?> GetProfileAsync(int userId)
    {
        var user = await _repo.GetUserAsync(userId);
        return user is null ? null : MapProfile(user);
    }

    public async Task<CustomerProfileDto?> UpdateProfileAsync(int userId, UpdateCustomerProfileDto request)
    {
        var user = await _repo.GetUserAsync(userId);
        if (user is null)
            return null;

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.DateOfBirth = request.DateOfBirth;
        user.Gender = request.Gender;
        user.Address = request.Address?.Trim();
        user.City = request.City?.Trim();
        user.Country = string.IsNullOrWhiteSpace(request.Country) ? "India" : request.Country.Trim();

        await _repo.UpdateUserAsync(user);
        return MapProfile(user);
    }

    public async Task<List<AccountDto>> GetAccountsAsync(int userId)
        => (await _repo.GetAccountsAsync(userId)).Select(MapAccount).ToList();

    public async Task<AccountDto> CreateAccountAsync(int userId, CreateAccountRequestDto request)
    {
        var accountType = NormalizeAccountType(request.AccountType);
        var account = new Account
        {
            UserId = userId,
            AccountNumber = await GenerateAccountNumberAsync(),
            AccountType = accountType,
            Balance = request.InitialDeposit,
            Currency = "INR",
            Status = "Active",
            MinimumBalance = accountType == "Current" ? 1000m : 500m,
            InterestRate = accountType == "Savings" ? 3.5m : 0m,
            BranchCode = "MAIN",
            Ifsccode = "SBIN000001",
            OpenedAt = DateTime.UtcNow
        };

        return MapAccount(await _repo.CreateAccountAsync(account));
    }

    private async Task<string> GenerateAccountNumberAsync()
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var candidate = $"SB{DateTime.UtcNow:yyMMddHHmmss}{Random.Shared.Next(10, 99)}";
            if (!await _repo.AccountNumberExistsAsync(candidate))
                return candidate;
        }

        throw new InvalidOperationException("Could not generate a unique account number.");
    }

    private static string NormalizeAccountType(string accountType)
    {
        var value = accountType.Trim();
        return value.Equals("Current", StringComparison.OrdinalIgnoreCase) ? "Current" : "Savings";
    }

    private static CustomerProfileDto MapProfile(User user) => new()
    {
        UserId = user.UserId,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        NationalId = user.NationalId,
        DateOfBirth = user.DateOfBirth,
        Gender = user.Gender,
        Address = user.Address,
        City = user.City,
        Country = user.Country,
        KycStatus = user.KycStatus ?? "Pending"
    };

    private static AccountDto MapAccount(Account account) => new()
    {
        AccountId = account.AccountId,
        AccountNumber = account.AccountNumber,
        AccountType = account.AccountType,
        Balance = account.Balance,
        Currency = account.Currency ?? "INR",
        Status = account.Status ?? "Active",
        MinimumBalance = account.MinimumBalance ?? 0m,
        InterestRate = account.InterestRate,
        BranchCode = account.BranchCode,
        Ifsccode = account.Ifsccode,
        OpenedAt = account.OpenedAt
    };
}
