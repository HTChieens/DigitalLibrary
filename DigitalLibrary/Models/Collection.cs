using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Collection
{
    public Guid Id { get; set; }

    public Guid CommunityId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? SubmissionPolicy { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CollectionDocument> CollectionDocuments { get; set; } = new List<CollectionDocument>();

    public virtual ICollection<CollectionPermission> CollectionPermissions { get; set; } = new List<CollectionPermission>();

    public virtual Community Community { get; set; } = null!;

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
