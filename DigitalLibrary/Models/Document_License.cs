using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DigitalLibrary.Models;

public partial class Document_License
{
    public string DocumentID { get; set; } = null!;

    public Guid LicenseID { get; set; }

    public DateTime AcceptedAt { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual License License { get; set; } = null!;
}
