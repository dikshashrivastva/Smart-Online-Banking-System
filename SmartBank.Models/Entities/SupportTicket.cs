using System;
using System.Collections.Generic;

namespace SmartBank.Models.Entities;

public partial class SupportTicket
{
    public int TicketId { get; set; }

    public int CreatedByUserId { get; set; }

    public int? AssignedToUserId { get; set; }

    public string Subject { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Category { get; set; }

    public string? Priority { get; set; }

    public string? Status { get; set; }

    public string? Resolution { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public virtual User? AssignedToUser { get; set; }

    public virtual User CreatedByUser { get; set; } = null!;
}
