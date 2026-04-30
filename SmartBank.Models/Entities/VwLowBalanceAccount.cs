using System;
using System.Collections.Generic;

namespace SmartBank.Models.Entities;

public partial class VwLowBalanceAccount
{
    public int AccountId { get; set; }

    public string AccountNumber { get; set; } = null!;

    public string AccountType { get; set; } = null!;

    public decimal Balance { get; set; }

    public decimal? MinimumBalance { get; set; }

    public decimal? BufferAmount { get; set; }

    public string CustomerName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }
}
