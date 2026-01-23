using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class License
{
    public Guid ID { get; set; }

    public string Name { get; set; } = null!;

    public string Content { get; set; } = null!;

    public virtual ICollection<Document_License> Document_Licenses { get; set; } = new List<Document_License>();
}
