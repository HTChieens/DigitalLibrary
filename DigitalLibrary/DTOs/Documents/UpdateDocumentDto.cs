namespace DigitalLibrary.DTOs.Documents
{
    public class UpdateDocumentDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string DocumentType { get; set; } = null!;
        public DateOnly PublicationDate { get; set; }
        public int PageNum { get; set; }
        public int IntroEndPage { get; set; }
        public string CoverPath { get; set; } = null!;

        public InternalBookDto? InternalBook { get; set; }
        public ExternalBookDto? ExternalBook { get; set; }
        public ThesisDto? Thesis { get; set; }
        public ResearchDto? Research { get; set; }
        public ResearchPublicationDto? ResearchPublication { get; set; }

        public List<Guid> LicenseIds { get; set; } = null!;
        public List<string> Keywords { get; set; } = null!;
        public List<IdentifierDto> Identifiers { get; set; } = null!;
        public List<AuthorInputDto> Authors { get; set; } = null!;

        public Guid CollectionId { get; set; }

        public List<LicenseInputDto>? Licenses { get; set; }

        public string Comment { get; set; } = null!;
    }
}
