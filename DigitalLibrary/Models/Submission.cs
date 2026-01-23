using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Submission
{
    public Guid ID { get; set; }

    public string DocumentID { get; set; } = null!;

    public Guid CollectionID { get; set; }

    public string SubmitterID { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int CurrentStep { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Collection Collection { get; set; } = null!;

    public virtual Document Document { get; set; } = null!;

    public virtual ICollection<Submission_History> Submission_Histories { get; set; } = new List<Submission_History>();

    public virtual User Submitter { get; set; } = null!;
}
