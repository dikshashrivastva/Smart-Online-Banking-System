using System;
using System.Collections.Generic;

namespace SmartBank.Models.Entities;

public partial class VwUnreadNotification
{
    public int UserId { get; set; }

    public int? UnreadCount { get; set; }

    public DateTime? OldestUnread { get; set; }

    public DateTime? LatestUnread { get; set; }
}
