using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class RefreshToken
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = null!;

    public string TokenHash { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
