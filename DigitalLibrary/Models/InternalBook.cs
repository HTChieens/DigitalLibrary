using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class InternalBook
{
    public string DocumentId { get; set; } = null!;

    public string Faculty { get; set; } = null!;

    public string DocumentType { get; set; } = null!;

    public int Version { get; set; }

    public virtual Document Document { get; set; } = null!;
}
