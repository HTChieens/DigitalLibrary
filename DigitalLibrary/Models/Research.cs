using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Research
{
    public string DocumentId { get; set; } = null!;

    public string? Abstract { get; set; }

    public string ResearchLevel { get; set; } = null!;

    public virtual Document Document { get; set; } = null!;
}
