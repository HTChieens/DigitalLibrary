using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Download
{
    public long Id { get; set; }

    public string UserId { get; set; } = null!;

    public string DocumentId { get; set; } = null!;

    public DateTime DownloadedAt { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
