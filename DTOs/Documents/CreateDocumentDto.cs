namespace DigitalLibrary.DTOs.Documents
{
    public class CreateDocumentDto
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

        public List<Guid> LicenseIds { get; set; } = [];
        public List<string> Keywords { get; set; } = [];
        public List<IdentifierDto> Identifiers { get; set; } = [];
        public List<AuthorInputDto> Authors { get; set; } = [];

        public Guid CollectionId { get; set; }

        public string FilePath { get; set; } = null!;
        public List<LicenseInputDto> Licenses { get; set; } = [];


    }
}
