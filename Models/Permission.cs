using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Permission
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<CollectionPermission> CollectionPermissions { get; set; } = new List<CollectionPermission>();
}
