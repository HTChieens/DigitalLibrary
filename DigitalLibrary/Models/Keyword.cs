using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Keyword
{
    public string ID { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
