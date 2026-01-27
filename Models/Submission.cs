using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Submission
{
    public Guid Id { get; set; }

    public string DocumentId { get; set; } = null!;

    public Guid CollectionId { get; set; }

    public string SubmitterId { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int CurrentStep { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Collection Collection { get; set; } = null!;

    public virtual Document Document { get; set; } = null!;

    public virtual ICollection<SubmissionHistory> SubmissionHistories { get; set; } = new List<SubmissionHistory>();

    public virtual User Submitter { get; set; } = null!;
}
