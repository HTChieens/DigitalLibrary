using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class SubmissionHistory
{
    public Guid Id { get; set; }

    public Guid SubmissionId { get; set; }

    public string PerformedBy { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User PerformedByNavigation { get; set; } = null!;

    public virtual Submission Submission { get; set; } = null!;
}
