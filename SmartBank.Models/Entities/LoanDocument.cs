using System;
using System.Collections.Generic;

namespace SmartBank.Models.Entities;

public partial class LoanDocument
{
    public int LoanDocumentId { get; set; }

    public int LoanId { get; set; }

    public string? DocumentType { get; set; }

    public string FilePath { get; set; } = null!;

    public DateTime? UploadedAt { get; set; }

    public virtual Loan Loan { get; set; } = null!;
}
