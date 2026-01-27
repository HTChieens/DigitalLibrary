using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class CollectionDocument
{
    public Guid CollectionId { get; set; }

    public string DocumentId { get; set; } = null!;

    public DateTime AddedAt { get; set; }

    public virtual Collection Collection { get; set; } = null!;

    public virtual Document Document { get; set; } = null!;
}
