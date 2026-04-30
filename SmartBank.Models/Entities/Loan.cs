using System;
using System.Collections.Generic;

namespace SmartBank.Models.Entities;

public partial class Loan
{
    public int LoanId { get; set; }

    public int UserId { get; set; }

    public int? AccountId { get; set; }

    public int? ReviewedByUserId { get; set; }

    public string? LoanType { get; set; }

    public decimal RequestedAmount { get; set; }

    public decimal? ApprovedAmount { get; set; }

    public decimal? InterestRate { get; set; }

    public int TenureMonths { get; set; }

    public decimal? Emiamount { get; set; }

    public string? Purpose { get; set; }

    public string? Status { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime? DisbursedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Account? Account { get; set; }

    public virtual ICollection<LoanDocument> LoanDocuments { get; set; } = new List<LoanDocument>();

    public virtual User? ReviewedByUser { get; set; }

    public virtual User User { get; set; } = null!;
}
