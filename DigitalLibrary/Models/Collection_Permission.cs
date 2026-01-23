using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Collection_Permission
{
    public Guid CollectionID { get; set; }

    public string RoleID { get; set; } = null!;

    public string PermissionID { get; set; } = null!;

    public virtual Collection Collection { get; set; } = null!;

    public virtual Permission Permission { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
