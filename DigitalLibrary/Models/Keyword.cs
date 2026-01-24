using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Keyword
{
    public string ID { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Doc_Keyword> Doc_Keywords { get; set; } = new List<Doc_Keyword>();
}
