using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBank.Data.Context;
using SmartBank.Models.DTOs.Customer;
using SmartBank.Models.DTOs.Service;
using SmartBank.Models.DTOs.Transactions;
using SmartBank.Models.Entities;

namespace SmartBank.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly SmartOnlineBankingDbContext _db;

    public AdminController(SmartOnlineBankingDbContext db)
    {
        _db = db;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var data = new AdminDashboardDto
        {
            TotalUsers = await _db.Users.CountAsync(),
            FrozenUsers = await _db.Users.CountAsync(u => u.IsFrozen == true),
            PendingLoans = await _db.Loans.CountAsync(l => l.Status == "Pending"),
            OpenTickets = await _db.SupportTickets.CountAsync(t => t.Status != "Closed"),
            TotalDeposits = await _db.Accounts.SumAsync(a => (decimal?)a.Balance) ?? 0m
        };

        return Ok(ApiResponse(true, "Admin dashboard loaded.", data));
    }

    [HttpGet("users")]
    public async Task<IActionResult> Users()
    {
        var users = await _db.Users
            .Include(u => u.Accounts)
            .Include(u => u.Role)
            .Where(u => u.Role.RoleName == "Customer")
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();

        var result = users.Select(u => new AdminCustomerAccountDto
        {
            UserId = u.UserId,
            FullName = $"{u.FirstName} {u.LastName}",
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            KycStatus = u.KycStatus,
            IsActive = u.IsActive ?? false,
            IsFrozen = u.IsFrozen ?? false,
            TotalBalance = u.Accounts.Sum(a => a.Balance),
            Accounts = u.Accounts
                .OrderBy(a => a.AccountNumber)
                .Select(a => new AccountDto
                {
                    AccountId = a.AccountId,
                    AccountNumber = a.AccountNumber,
                    AccountType = a.AccountType,
                    Balance = a.Balance,
                    Currency = a.Currency ?? "INR",
                    Status = a.Status ?? string.Empty,
                    MinimumBalance = a.MinimumBalance ?? 0m,
                    InterestRate = a.InterestRate,
                    BranchCode = a.BranchCode,
                    Ifsccode = a.Ifsccode,
                    OpenedAt = a.OpenedAt
                })
                .ToList()
        }).ToList();

        return Ok(ApiResponse(true, "Users loaded.", result));
    }

    [HttpPost("freeze")]
    public async Task<IActionResult> Freeze(FreezeUserRequestDto request)
    {
        var user = await _db.Users.FindAsync(request.UserId);
        if (user is null)
            return NotFound(ApiResponse(false, "User not found.", null));

        user.IsFrozen = request.Freeze;
        user.UpdatedAt = DateTime.UtcNow;

        _db.Notifications.Add(new Notification
        {
            UserId = user.UserId,
            Title = request.Freeze ? "Account frozen" : "Account unfrozen",
            Message = request.Freeze
                ? "Your login and banking access has been frozen by admin."
                : "Your banking access has been restored by admin.",
            Type = "Admin",
            IsRead = false,
            RelatedEntityId = user.UserId,
            RelatedEntityType = "User",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(ApiResponse(true, request.Freeze ? "User frozen." : "User unfrozen.", null));
    }

    [HttpPost("users/kyc")]
    public async Task<IActionResult> UpdateKycStatus(UpdateKycStatusRequestDto request)
    {
        var user = await _db.Users.FindAsync(request.UserId);
        if (user is null)
            return NotFound(ApiResponse(false, "User not found.", null));

        user.KycStatus = request.KycStatus.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        _db.Notifications.Add(new Notification
        {
            UserId = user.UserId,
            Title = "KYC status updated",
            Message = $"Your KYC status is now {user.KycStatus}.",
            Type = "KYC",
            IsRead = false,
            RelatedEntityId = user.UserId,
            RelatedEntityType = "User",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(ApiResponse(true, "KYC status updated.", null));
    }

    [HttpPost("accounts/status")]
    public async Task<IActionResult> UpdateAccountStatus(AccountStatusRequestDto request)
    {
        var allowedStatuses = new[] { "Active", "Frozen", "Closed" };
        if (!allowedStatuses.Contains(request.Status, StringComparer.OrdinalIgnoreCase))
            return BadRequest(ApiResponse(false, "Invalid account status.", null));

        var account = await _db.Accounts.Include(a => a.User).FirstOrDefaultAsync(a => a.AccountId == request.AccountId);
        if (account is null)
            return NotFound(ApiResponse(false, "Account not found.", null));

        var status = allowedStatuses.First(s => s.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
        account.Status = status;
        account.ClosedAt = status == "Closed" ? DateTime.UtcNow : null;
        account.UpdatedAt = DateTime.UtcNow;

        _db.Notifications.Add(new Notification
        {
            UserId = account.UserId,
            Title = $"Account {status.ToLower()}",
            Message = $"Your account {account.AccountNumber} has been marked {status} by admin.",
            Type = "Account",
            IsRead = false,
            RelatedEntityId = account.AccountId,
            RelatedEntityType = "Account",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(ApiResponse(true, $"Account marked {status}.", null));
    }

    [HttpGet("loans")]
    public async Task<IActionResult> Loans()
    {
        var loans = await _db.Loans
            .Include(l => l.User)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse(true, "Loans loaded.", loans.Select(l => new AdminLoanDto
        {
            LoanId = l.LoanId,
            UserId = l.UserId,
            CustomerName = $"{l.User.FirstName} {l.User.LastName}",
            CustomerEmail = l.User.Email,
            LoanType = l.LoanType ?? string.Empty,
            RequestedAmount = l.RequestedAmount,
            ApprovedAmount = l.ApprovedAmount,
            InterestRate = l.InterestRate,
            TenureMonths = l.TenureMonths,
            Emiamount = l.Emiamount,
            Status = l.Status ?? "Pending",
            Purpose = l.Purpose,
            RejectionReason = l.RejectionReason,
            CreatedAt = l.CreatedAt,
            ReviewedAt = l.ReviewedAt
        }).ToList()));
    }

    [HttpPost("loan/approve")]
    public async Task<IActionResult> DecideLoan(LoanDecisionRequestDto request)
    {
        var loan = await _db.Loans.Include(l => l.User).FirstOrDefaultAsync(l => l.LoanId == request.LoanId);
        if (loan is null)
            return NotFound(ApiResponse(false, "Loan not found.", null));

        loan.ReviewedByUserId = GetUserId();
        loan.ReviewedAt = DateTime.UtcNow;
        loan.UpdatedAt = DateTime.UtcNow;

        if (request.Approve)
        {
            loan.Status = "Approved";
            loan.ApprovedAmount = request.ApprovedAmount.GetValueOrDefault(loan.RequestedAmount);
            loan.InterestRate = request.InterestRate.GetValueOrDefault(10.5m);
            loan.Emiamount = CalculateEmi(loan.ApprovedAmount.Value, loan.InterestRate.Value, loan.TenureMonths);
            loan.RejectionReason = null;
        }
        else
        {
            loan.Status = "Rejected";
            loan.ApprovedAmount = null;
            loan.InterestRate = null;
            loan.Emiamount = null;
            loan.RejectionReason = request.RejectionReason;
        }

        _db.Notifications.Add(new Notification
        {
            UserId = loan.UserId,
            Title = request.Approve ? "Loan approved" : "Loan rejected",
            Message = request.Approve
                ? $"Your {loan.LoanType} loan has been approved for INR {loan.ApprovedAmount:N2}."
                : $"Your {loan.LoanType} loan was rejected. {loan.RejectionReason}",
            Type = "Loan",
            IsRead = false,
            RelatedEntityId = loan.LoanId,
            RelatedEntityType = "Loan",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(ApiResponse(true, request.Approve ? "Loan approved." : "Loan rejected.", null));
    }

    [HttpGet("tickets")]
    public async Task<IActionResult> Tickets()
    {
        var tickets = await _db.SupportTickets
            .Include(t => t.CreatedByUser)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse(true, "Tickets loaded.", tickets.Select(MapTicket).ToList()));
    }

    [HttpPost("tickets/{ticketId:int}/reply")]
    public async Task<IActionResult> ReplyTicket(int ticketId, TicketUpdateRequestDto request)
    {
        var ticket = await _db.SupportTickets.FindAsync(ticketId);
        if (ticket is null)
            return NotFound(ApiResponse(false, "Ticket not found.", null));

        ticket.AssignedToUserId = GetUserId();
        ticket.Resolution = request.Resolution.Trim();
        ticket.Status = request.Status.Trim();
        ticket.UpdatedAt = DateTime.UtcNow;
        ticket.ResolvedAt = ticket.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase) ? DateTime.UtcNow : null;

        _db.Notifications.Add(new Notification
        {
            UserId = ticket.CreatedByUserId,
            Title = $"Ticket #{ticket.TicketId} updated",
            Message = ticket.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase)
                ? $"Admin closed your ticket: {ticket.Resolution}"
                : $"Admin replied to your ticket: {ticket.Resolution}",
            Type = "Ticket",
            IsRead = false,
            RelatedEntityId = ticket.TicketId,
            RelatedEntityType = "SupportTicket",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(ApiResponse(true, "Ticket updated.", null));
    }

    [HttpGet("reports")]
    public async Task<IActionResult> Reports([FromQuery] int? userId)
    {
        var users = await _db.Users
            .Include(u => u.Accounts)
            .Where(u => userId == null || u.UserId == userId)
            .OrderBy(u => u.FirstName)
            .ToListAsync();

        var reports = new List<CustomerReportDto>();
        foreach (var user in users)
        {
            var accountIds = user.Accounts.Select(a => a.AccountId).ToList();
            var transactions = await _db.Transactions
                .Where(t => accountIds.Contains(t.AccountId))
                .OrderByDescending(t => t.CreatedAt)
                .Take(50)
                .ToListAsync();

            reports.Add(new CustomerReportDto
            {
                UserId = user.UserId,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                KycStatus = user.KycStatus,
                IsActive = user.IsActive ?? false,
                IsFrozen = user.IsFrozen ?? false,
                TotalBalance = user.Accounts.Sum(a => a.Balance),
                Accounts = user.Accounts.Select(a => new AccountDto
                {
                    AccountId = a.AccountId,
                    AccountNumber = a.AccountNumber,
                    AccountType = a.AccountType,
                    Balance = a.Balance,
                    Currency = a.Currency ?? "INR",
                    Status = a.Status ?? string.Empty,
                    MinimumBalance = a.MinimumBalance ?? 0m,
                    InterestRate = a.InterestRate,
                    BranchCode = a.BranchCode,
                    Ifsccode = a.Ifsccode,
                    OpenedAt = a.OpenedAt
                }).ToList(),
                Transactions = transactions.Select(t => new TransactionResponseDto
                {
                    Id = t.TransactionId,
                    ReferenceNumber = t.ReferenceNumber ?? string.Empty,
                    Type = t.TransactionType ?? string.Empty,
                    Amount = t.Amount,
                    BalanceBefore = t.BalanceBefore,
                    BalanceAfter = t.BalanceAfter,
                    Status = t.Status ?? string.Empty,
                    CreatedAt = t.CreatedAt ?? DateTime.MinValue
                }).ToList(),
                Loans = await _db.Loans.Where(l => l.UserId == user.UserId).OrderByDescending(l => l.CreatedAt).Select(l => new LoanStatusDto
                {
                    LoanId = l.LoanId,
                    LoanType = l.LoanType ?? string.Empty,
                    RequestedAmount = l.RequestedAmount,
                    ApprovedAmount = l.ApprovedAmount,
                    InterestRate = l.InterestRate,
                    TenureMonths = l.TenureMonths,
                    Emiamount = l.Emiamount,
                    Status = l.Status ?? "Pending",
                    Purpose = l.Purpose,
                    RejectionReason = l.RejectionReason,
                    CreatedAt = l.CreatedAt,
                    ReviewedAt = l.ReviewedAt
                }).ToListAsync(),
                Tickets = await _db.SupportTickets.Where(t => t.CreatedByUserId == user.UserId).OrderByDescending(t => t.CreatedAt).Select(t => new TicketDto
                {
                    TicketId = t.TicketId,
                    CreatedByUserId = t.CreatedByUserId,
                    Subject = t.Subject,
                    Description = t.Description,
                    Category = t.Category,
                    Priority = t.Priority ?? "Normal",
                    Status = t.Status ?? "Open",
                    Resolution = t.Resolution,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    ResolvedAt = t.ResolvedAt
                }).ToListAsync()
            });
        }

        return Ok(ApiResponse(true, "Reports generated.", reports));
    }

    private int GetUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    private static decimal CalculateEmi(decimal principal, decimal yearlyRate, int months)
    {
        if (months <= 0)
            return principal;

        var monthlyRate = yearlyRate / 12m / 100m;
        if (monthlyRate <= 0)
            return decimal.Round(principal / months, 2);

        var rate = (double)monthlyRate;
        var power = Math.Pow(1 + rate, months);
        return decimal.Round(principal * (decimal)(rate * power / (power - 1)), 2);
    }

    private static TicketDto MapTicket(SupportTicket ticket) => new()
    {
        TicketId = ticket.TicketId,
        CreatedByUserId = ticket.CreatedByUserId,
        CustomerName = $"{ticket.CreatedByUser.FirstName} {ticket.CreatedByUser.LastName}",
        CustomerEmail = ticket.CreatedByUser.Email,
        Subject = ticket.Subject,
        Description = ticket.Description,
        Category = ticket.Category,
        Priority = ticket.Priority ?? "Normal",
        Status = ticket.Status ?? "Open",
        Resolution = ticket.Resolution,
        CreatedAt = ticket.CreatedAt,
        UpdatedAt = ticket.UpdatedAt,
        ResolvedAt = ticket.ResolvedAt
    };

    private static object ApiResponse(bool success, string message, object? data)
        => new { success, message, data };
}
