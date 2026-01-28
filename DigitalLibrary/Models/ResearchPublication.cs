using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class ResearchPublication
{
    public string DocumentId { get; set; } = null!;

    public string VenueName { get; set; } = null!;

    public string PublicationType { get; set; } = null!;

    public virtual Document Document { get; set; } = null!;
}
