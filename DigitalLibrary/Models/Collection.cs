using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Collection
{
    public Guid ID { get; set; }

    public Guid CommunityID { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? SubmissionPolicy { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Collection_Document> Collection_Documents { get; set; } = new List<Collection_Document>();

    public virtual ICollection<Collection_Permission> Collection_Permissions { get; set; } = new List<Collection_Permission>();

    public virtual Community Community { get; set; } = null!;

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
