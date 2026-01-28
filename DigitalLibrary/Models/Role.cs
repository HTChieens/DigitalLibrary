using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Role
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<CollectionPermission> CollectionPermissions { get; set; } = new List<CollectionPermission>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
