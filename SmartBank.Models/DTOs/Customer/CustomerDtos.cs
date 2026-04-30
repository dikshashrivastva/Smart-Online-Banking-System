using System.ComponentModel.DataAnnotations;

namespace SmartBank.Models.DTOs.Customer;

public class CustomerProfileDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? NationalId { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string KycStatus { get; set; } = string.Empty;
}

public class UpdateCustomerProfileDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }
}

public class CreateAccountRequestDto
{
    [Required]
    [MaxLength(20)]
    public string AccountType { get; set; } = "Savings";

    [Range(0, 100000000)]
    public decimal InitialDeposit { get; set; }
}

public class AccountDto
{
    public int AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "INR";
    public string Status { get; set; } = string.Empty;
    public decimal MinimumBalance { get; set; }
    public decimal? InterestRate { get; set; }
    public string? BranchCode { get; set; }
    public string? Ifsccode { get; set; }
    public DateTime? OpenedAt { get; set; }
}
