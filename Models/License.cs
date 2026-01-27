using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class License
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Content { get; set; } = null!;

    public virtual ICollection<DocumentLicense> DocumentLicenses { get; set; } = new List<DocumentLicense>();
}
