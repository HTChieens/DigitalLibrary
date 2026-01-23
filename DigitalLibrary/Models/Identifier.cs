using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Identifier
{
    public Guid ID { get; set; }

    public string DocumentID { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string Value { get; set; } = null!;

    public virtual Document Document { get; set; } = null!;
}
