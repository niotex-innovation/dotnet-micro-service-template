using System;
using System.Collections.Generic;

namespace PROJECT_TEMPLATE.Database;

public partial class Invitation
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string WorkEmail { get; set; } = null!;

    public string FirmSize { get; set; } = null!;

    public string? ReferralCode { get; set; }

    public string? PromotionalCode { get; set; }

    public Guid? OutboundEmailId { get; set; }

    public bool? InvitationAccepted { get; set; }

    public string? FirebaseUserId { get; set; }

    public DateTime CreatedAt { get; set; }
}
