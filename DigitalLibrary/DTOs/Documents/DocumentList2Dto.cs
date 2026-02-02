namespace DigitalLibrary.DTOs.Documents
{
    public class DocumentList2Dto
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string DocumentType { get; set; } = null!;
        public DateTime PublicationDate { get; set; }
        public string? CoverPath { get; set; }

        public int ViewCount { get; set; }
    }
}
