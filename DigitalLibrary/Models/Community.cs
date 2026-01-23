using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Community
{
    public Guid ID { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public Guid? ParentCommunityID { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Collection> Collections { get; set; } = new List<Collection>();

    public virtual ICollection<Community> InverseParentCommunity { get; set; } = new List<Community>();

    public virtual Community? ParentCommunity { get; set; }
}
