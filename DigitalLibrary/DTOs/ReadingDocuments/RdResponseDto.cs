
namespace DigitalLibrary.DTOs.ReadingDocuments
{
    public class RdResponseDto
    {

        public string DocumentId { get; set; } = null!;
        public string Title{ get; set; } = null!;
        public string CoverPath{ get; set; } = null!;
        public int CurrentPage { get; set; }

        public DateTime LastReadAt { get; set; }

        public DateTime FirstReadAt { get; set; }

        //public bool? IsCounted { get; set; }

    }
}
