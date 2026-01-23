using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class User_Author
{
    public string UserID { get; set; } = null!;

    public string AuthorID { get; set; } = null!;

    public virtual Author Author { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
