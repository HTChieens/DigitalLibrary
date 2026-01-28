using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Thesis
{
    public string DocumentId { get; set; } = null!;

    public string DegreeLevel { get; set; } = null!;

    public string Discipline { get; set; } = null!;

    public string AdvisorName { get; set; } = null!;

    public string? Abstract { get; set; }

    public virtual Document Document { get; set; } = null!;
}
