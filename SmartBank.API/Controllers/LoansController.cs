using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBank.Data.Context;
using SmartBank.Models.DTOs.Service;
using SmartBank.Models.Entities;

namespace SmartBank.API.Controllers;

[ApiController]
[Authorize]
[Route("api/loans")]
public class LoansController : ControllerBase
{
    private readonly SmartOnlineBankingDbContext _db;

    public LoansController(SmartOnlineBankingDbContext db)
    {
        _db = db;
    }

    [HttpPost("apply")]
    public async Task<IActionResult> Apply(LoanApplyRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (request.AccountId.HasValue)
        {
            var ownsAccount = await _db.Accounts.AnyAsync(a => a.AccountId == request.AccountId && a.UserId == userId);
            if (!ownsAccount)
                return BadRequest(new { success = false, message = "Selected account was not found.", data = (object?)null });
        }

        var loan = new Loan
        {
            UserId = userId,
            AccountId = request.AccountId,
            LoanType = request.LoanType.Trim(),
            RequestedAmount = request.RequestedAmount,
            TenureMonths = request.TenureMonths,
            Purpose = request.Purpose?.Trim(),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();

        await AddNotificationAsync(userId, "Loan application submitted", $"Your {loan.LoanType} loan request is pending review.", "Loan", loan.LoanId, "Loan");
        await _db.SaveChangesAsync();

        return Ok(ApiResponse(true, "Loan request submitted.", MapLoan(loan)));
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status()
    {
        var loans = await _db.Loans
            .Where(l => l.UserId == GetUserId())
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse(true, "Loan status loaded.", loans.Select(MapLoan).ToList()));
    }

    private Task AddNotificationAsync(int userId, string title, string message, string type, int relatedId, string relatedType)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            RelatedEntityId = relatedId,
            RelatedEntityType = relatedType,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        return Task.CompletedTask;
    }

    private int GetUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    private static LoanStatusDto MapLoan(Loan loan) => new()
    {
        LoanId = loan.LoanId,
        LoanType = loan.LoanType ?? string.Empty,
        RequestedAmount = loan.RequestedAmount,
        ApprovedAmount = loan.ApprovedAmount,
        InterestRate = loan.InterestRate,
        TenureMonths = loan.TenureMonths,
        Emiamount = loan.Emiamount,
        Status = loan.Status ?? "Pending",
        Purpose = loan.Purpose,
        RejectionReason = loan.RejectionReason,
        CreatedAt = loan.CreatedAt,
        ReviewedAt = loan.ReviewedAt
    };

    private static object ApiResponse(bool success, string message, object? data)
        => new { success, message, data };
}
