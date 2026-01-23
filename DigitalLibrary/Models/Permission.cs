using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Permission
{
    public string ID { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Collection_Permission> Collection_Permissions { get; set; } = new List<Collection_Permission>();
}
