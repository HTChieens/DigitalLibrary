using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class ReadingDocument
{
    public string UserID { get; set; } = null!;

    public string DocumentID { get; set; } = null!;

    public int CurrentPage { get; set; }

    public DateTime LastReadAt { get; set; }

    public DateTime FirstReadAt { get; set; }

    public bool IsCounted { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
