using System.ComponentModel.DataAnnotations;
using SmartBank.Models.DTOs.Customer;
using SmartBank.Models.DTOs.Transactions;

namespace SmartBank.Models.DTOs.Service;

public class LoanApplyRequestDto
{
    public int? AccountId { get; set; }

    [Required]
    [MaxLength(50)]
    public string LoanType { get; set; } = "Personal";

    [Range(1000, 100000000)]
    public decimal RequestedAmount { get; set; }

    [Range(1, 360)]
    public int TenureMonths { get; set; }

    [MaxLength(500)]
    public string? Purpose { get; set; }
}

public class LoanStatusDto
{
    public int LoanId { get; set; }
    public string LoanType { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public decimal? InterestRate { get; set; }
    public int TenureMonths { get; set; }
    public decimal? Emiamount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Purpose { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class TicketCreateRequestDto
{
    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Category { get; set; }

    [MaxLength(20)]
    public string Priority { get; set; } = "Normal";
}

public class TicketDto
{
    public int TicketId { get; set; }
    public int CreatedByUserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class TicketUpdateRequestDto
{
    [Required]
    [MaxLength(2000)]
    public string Resolution { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Open";
}

public class NotificationDto
{
    public int NotificationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool IsRead { get; set; }
    public int? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class AdminLoanDto : LoanStatusDto
{
    public int UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
}

public class LoanDecisionRequestDto
{
    public int LoanId { get; set; }
    public bool Approve { get; set; } = true;

    [Range(0, 100000000)]
    public decimal? ApprovedAmount { get; set; }

    [Range(0, 100)]
    public decimal? InterestRate { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }
}

public class FreezeUserRequestDto
{
    public int UserId { get; set; }
    public bool Freeze { get; set; }
}

public class UpdateKycStatusRequestDto
{
    public int UserId { get; set; }

    [Required]
    [MaxLength(20)]
    public string KycStatus { get; set; } = "Pending";
}

public class AccountStatusRequestDto
{
    public int AccountId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Active";
}

public class AdminCustomerAccountDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? KycStatus { get; set; }
    public bool IsActive { get; set; }
    public bool IsFrozen { get; set; }
    public decimal TotalBalance { get; set; }
    public List<AccountDto> Accounts { get; set; } = [];
}

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int FrozenUsers { get; set; }
    public int PendingLoans { get; set; }
    public int OpenTickets { get; set; }
    public decimal TotalDeposits { get; set; }
}

public class CustomerReportDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? KycStatus { get; set; }
    public bool IsActive { get; set; }
    public bool IsFrozen { get; set; }
    public List<AccountDto> Accounts { get; set; } = [];
    public List<TransactionResponseDto> Transactions { get; set; } = [];
    public List<LoanStatusDto> Loans { get; set; } = [];
    public List<TicketDto> Tickets { get; set; } = [];
    public decimal TotalBalance { get; set; }
}
