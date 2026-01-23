using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Review
{
    public long ID { get; set; }

    public string DocumentID { get; set; } = null!;

    public string UserID { get; set; } = null!;

    public byte? Rating { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Content { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
