namespace DigitalLibrary.Models
{
    public class DocumentFile
    {
        public Guid Id { get; set; }

        public string DocumentId { get; set; } = null!;
        public Document Document { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public int Version { get; set; }
        public string? ChangeNote { get; set; }
    }
}
