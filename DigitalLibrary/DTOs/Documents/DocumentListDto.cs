namespace DigitalLibrary.DTOs.Documents
{
    public class DocumentListDto
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string DocumentType { get; set; } = null!;
        public DateTime PublicationDate { get; set; }
        public string? CoverPath { get; set; }
    }
}
