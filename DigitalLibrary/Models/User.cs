using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class User
{
    public string ID { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string RoleID { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? Class { get; set; }

    public virtual ICollection<Download> Downloads { get; set; } = new List<Download>();

    public virtual ICollection<ReadingDocument> ReadingDocuments { get; set; } = new List<ReadingDocument>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<SavedDocument> SavedDocuments { get; set; } = new List<SavedDocument>();

    public virtual ICollection<Submission_History> Submission_Histories { get; set; } = new List<Submission_History>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public virtual User_Author? User_Author { get; set; }
}
