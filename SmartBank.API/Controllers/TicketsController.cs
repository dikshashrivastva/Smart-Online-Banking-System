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
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly SmartOnlineBankingDbContext _db;

    public TicketsController(SmartOnlineBankingDbContext db)
    {
        _db = db;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(TicketCreateRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ticket = new SupportTicket
        {
            CreatedByUserId = GetUserId(),
            Subject = request.Subject.Trim(),
            Description = request.Description.Trim(),
            Category = request.Category?.Trim(),
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Normal" : request.Priority.Trim(),
            Status = "Open",
            CreatedAt = DateTime.UtcNow
        };

        _db.SupportTickets.Add(ticket);
        await _db.SaveChangesAsync();

        _db.Notifications.Add(new Notification
        {
            UserId = ticket.CreatedByUserId,
            Title = "Support ticket created",
            Message = $"Ticket #{ticket.TicketId} is open and awaiting admin review.",
            Type = "Ticket",
            IsRead = false,
            RelatedEntityId = ticket.TicketId,
            RelatedEntityType = "SupportTicket",
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Support ticket created.", data = MapTicket(ticket) });
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status()
    {
        var tickets = await _db.SupportTickets
            .Include(t => t.CreatedByUser)
            .Where(t => t.CreatedByUserId == GetUserId())
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(new { success = true, message = "Tickets loaded.", data = tickets.Select(MapTicket).ToList() });
    }

    private int GetUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    private static TicketDto MapTicket(SupportTicket ticket) => new()
    {
        TicketId = ticket.TicketId,
        CreatedByUserId = ticket.CreatedByUserId,
        CustomerName = ticket.CreatedByUser is null ? string.Empty : $"{ticket.CreatedByUser.FirstName} {ticket.CreatedByUser.LastName}",
        CustomerEmail = ticket.CreatedByUser?.Email ?? string.Empty,
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
}
