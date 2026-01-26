
namespace DigitalLibrary.DTOs.ReadingDocuments
{
    public class ReadingDocumentDto
    {

        public string DocumentID { get; set; } = null!;

        public int CurrentPage { get; set; }

        public DateTime LastReadAt { get; set; }

        public DateTime FirstReadAt { get; set; }

        public bool? IsCounted { get; set; }

    }
}
