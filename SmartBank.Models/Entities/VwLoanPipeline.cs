using System;
using System.Collections.Generic;

namespace SmartBank.Models.Entities;

public partial class VwLoanPipeline
{
    public int LoanId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerEmail { get; set; } = null!;

    public string? LoanType { get; set; }

    public decimal RequestedAmount { get; set; }

    public decimal? ApprovedAmount { get; set; }

    public decimal? InterestRate { get; set; }

    public int TenureMonths { get; set; }

    public decimal? Emiamount { get; set; }

    public string? Status { get; set; }

    public string? Purpose { get; set; }

    public DateTime? AppliedOn { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewedBy { get; set; }
}
