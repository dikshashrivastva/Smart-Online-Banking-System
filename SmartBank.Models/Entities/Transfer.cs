using System;
using System.Collections.Generic;

namespace SmartBank.Models.Entities;

public partial class Transfer
{
    public int TransferId { get; set; }

    public int FromAccountId { get; set; }

    public int ToAccountId { get; set; }

    public decimal Amount { get; set; }

    public string? Remarks { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Status { get; set; }

    public int InitiatedByUserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual Account FromAccount { get; set; } = null!;

    public virtual User InitiatedByUser { get; set; } = null!;

    public virtual Account ToAccount { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
