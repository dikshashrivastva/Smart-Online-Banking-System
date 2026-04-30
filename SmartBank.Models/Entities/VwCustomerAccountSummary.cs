using System;
using System.Collections.Generic;

namespace SmartBank.Models.Entities;

public partial class VwCustomerAccountSummary
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? KycStatus { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsFrozen { get; set; }

    public int? TotalAccounts { get; set; }

    public decimal? TotalBalance { get; set; }

    public int? SavingsAccounts { get; set; }

    public int? CurrentAccounts { get; set; }
}
