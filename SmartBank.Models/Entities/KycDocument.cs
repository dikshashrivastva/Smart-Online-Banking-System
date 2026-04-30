using System;
using System.Collections.Generic;

namespace SmartBank.Models.Entities;

public partial class KycDocument
{
    public int KycDocumentId { get; set; }

    public int UserId { get; set; }

    public string? DocumentType { get; set; }

    public string DocumentNumber { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public DateTime? UploadedAt { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public int? VerifiedByUserId { get; set; }

    public string? Status { get; set; }

    public string? RejectionReason { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual User? VerifiedByUser { get; set; }
}
