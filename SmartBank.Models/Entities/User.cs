using System;
using System.Collections.Generic;

namespace SmartBank.Models.Entities;

public partial class User
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? NationalId { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public string? KycStatus { get; set; }

    public bool? IsEmailVerified { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsFrozen { get; set; }

    public int? FailedLoginAttempts { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<KycDocument> KycDocumentUsers { get; set; } = new List<KycDocument>();

    public virtual ICollection<KycDocument> KycDocumentVerifiedByUsers { get; set; } = new List<KycDocument>();

    public virtual ICollection<Loan> LoanReviewedByUsers { get; set; } = new List<Loan>();

    public virtual ICollection<Loan> LoanUsers { get; set; } = new List<Loan>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<SupportTicket> SupportTicketAssignedToUsers { get; set; } = new List<SupportTicket>();

    public virtual ICollection<SupportTicket> SupportTicketCreatedByUsers { get; set; } = new List<SupportTicket>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();
}
