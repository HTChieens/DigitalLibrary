using System;
using System.Collections.Generic;

namespace DigitalLibrary.Models;

public partial class Document
{
    public string ID { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string DocumentType { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public DateOnly PublicationDate { get; set; }

    public int PageNum { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? IntroEndPage { get; set; }

    public string CoverPath { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<Collection_Document> Collection_Documents { get; set; } = new List<Collection_Document>();

    public virtual ICollection<Document_License> Document_Licenses { get; set; } = new List<Document_License>();

    public virtual ICollection<Download> Downloads { get; set; } = new List<Download>();

    public virtual ExternalBook? ExternalBook { get; set; }

    public virtual ICollection<Identifier> Identifiers { get; set; } = new List<Identifier>();

    public virtual InternalBook? InternalBook { get; set; }

    public virtual ICollection<ReadingDocument> ReadingDocuments { get; set; } = new List<ReadingDocument>();

    public virtual Research? Research { get; set; }

    public virtual ResearchPublication? ResearchPublication { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<SavedDocument> SavedDocuments { get; set; } = new List<SavedDocument>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public virtual Thesis? Thesis { get; set; }

    public virtual ICollection<Author> Authors { get; set; } = new List<Author>();

    public virtual ICollection<Doc_Keyword> Doc_Keywords { get; set; } = new List<Doc_Keyword>();
}
