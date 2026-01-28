using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class ExternalBook
{
    public string DocumentId { get; set; } = null!;

    public string Publisher { get; set; } = null!;

    public int Version { get; set; }

    public virtual Document Document { get; set; } = null!;
}
