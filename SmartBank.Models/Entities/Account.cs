using System;
using System.Collections.Generic;

namespace SmartBank.Models.Entities;

public partial class Account
{
    public int AccountId { get; set; }

    public int UserId { get; set; }

    public string AccountNumber { get; set; } = null!;

    public string AccountType { get; set; } = null!;

    public decimal Balance { get; set; }

    public string? Currency { get; set; }

    public string? Status { get; set; }

    public decimal? MinimumBalance { get; set; }

    public decimal? InterestRate { get; set; }

    public string? BranchCode { get; set; }

    public string? Ifsccode { get; set; }

    public DateTime? OpenedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<Transfer> TransferFromAccounts { get; set; } = new List<Transfer>();

    public virtual ICollection<Transfer> TransferToAccounts { get; set; } = new List<Transfer>();

    public virtual User User { get; set; } = null!;
}
