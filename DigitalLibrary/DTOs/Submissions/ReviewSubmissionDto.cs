namespace DigitalLibrary.DTOs.Submissions
{
    public class ReviewSubmissionDto
    {
        public Guid SubmissionId { get; set; }
        public string Comment { get; set; } = null!;
    }
}
