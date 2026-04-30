using System;
using System.Collections.Generic;

namespace SmartBank.Models.Entities;

public partial class VwDailyTransactionSummary
{
    public DateOnly? TransactionDate { get; set; }

    public string? TransactionType { get; set; }

    public int? TotalCount { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? AverageAmount { get; set; }

    public decimal? MinAmount { get; set; }

    public decimal? MaxAmount { get; set; }
}
