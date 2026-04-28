using System;
using System.Collections.Generic;

namespace SmartBank.Models.Entities;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public int AccountId { get; set; }

    public int? TransferId { get; set; }

    public int? PerformedByUserId { get; set; }

    public string? TransactionType { get; set; }

    public decimal Amount { get; set; }

    public decimal BalanceAfter { get; set; }

    public string? Description { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Channel { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual User? PerformedByUser { get; set; }

    public virtual Transfer? Transfer { get; set; }
}
