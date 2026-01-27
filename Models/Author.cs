using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Author
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public string? Description { get; set; }

    public string? Image { get; set; }

    public string? Expertise { get; set; }

    public string? Orcid { get; set; }

    public virtual UserAuthor? UserAuthor { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
