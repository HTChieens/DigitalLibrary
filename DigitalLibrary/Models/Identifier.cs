using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Identifier
{
    public Guid Id { get; set; }

    public string DocumentId { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string Value { get; set; } = null!;

    public virtual Document Document { get; set; } = null!;
}
