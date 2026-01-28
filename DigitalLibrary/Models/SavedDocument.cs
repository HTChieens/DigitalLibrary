using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class SavedDocument
{
    public string UserId { get; set; } = null!;

    public string DocumentId { get; set; } = null!;

    public DateTime SavedAt { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
