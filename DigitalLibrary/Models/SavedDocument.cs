using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class SavedDocument
{
    public string UserID { get; set; } = null!;

    public string DocumentID { get; set; } = null!;

    public DateTime SavedAt { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
