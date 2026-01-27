using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class DocumentLicense
{
    public string DocumentId { get; set; } = null!;

    public Guid LicenseId { get; set; }

    public DateTime AcceptedAt { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual License License { get; set; } = null!;
}
