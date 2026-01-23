using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Collection_Document
{
    public Guid CollectionID { get; set; }

    public string DocumentID { get; set; } = null!;

    public DateTime AddedAt { get; set; }

    public virtual Collection Collection { get; set; } = null!;

    public virtual Document Document { get; set; } = null!;
}
