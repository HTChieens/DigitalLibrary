using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class UserAuthor
{
    public string UserId { get; set; } = null!;

    public string AuthorId { get; set; } = null!;

    public virtual Author Author { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
