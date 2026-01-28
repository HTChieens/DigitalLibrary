namespace DigitalLibrary.DTOs.Submissions
{
    public class CreateSubmissionDto
    {
        public string DocumentId { get; set; } = null!;
        public Guid CollectionId { get; set; }
    }
}
