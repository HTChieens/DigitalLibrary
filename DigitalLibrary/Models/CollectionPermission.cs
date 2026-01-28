using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class CollectionPermission
{
    public Guid CollectionId { get; set; }

    public string RoleId { get; set; } = null!;

    public string PermissionId { get; set; } = null!;

    public virtual Collection Collection { get; set; } = null!;

    public virtual Permission Permission { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
