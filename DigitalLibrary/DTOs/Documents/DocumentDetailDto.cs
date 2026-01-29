using DigitalLibrary.DTOs.Licenses;

namespace DigitalLibrary.DTOs.Documents
{
    public class DocumentDetailDto
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string DocumentType { get; set; } = null!;
        public int PageNum { get; set; }
        public DateOnly PublicationDate { get; set; }

        public InternalBookDto? InternalBook { get; set; }
        public ThesisDto? Thesis { get; set; }
        public ResearchDto? Research { get; set; }
        public ExternalBookDto? ExternalBook { get; set; }
        public ResearchPublicationDto? ResearchPublication { get; set; }

        public List<string> Authors { get; set; } = new();
        public List<string> Keywords { get; set; } = new();
        public List<string> Identifiers { get; set; } = new();
        public List<LicenseDto> Licenses { get; set; } = new();
    }
}
