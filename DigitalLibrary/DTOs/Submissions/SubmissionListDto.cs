namespace DigitalLibrary.DTOs.Submissions
{
    public class SubmissionListDto
    {
        public Guid SubmissionId { get; set; }
        public string DocumentTitle { get; set; } = null!;
        public string DocumentType { get; set; } = null!;
        public string CollectionName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = null!;

        public int CurrentStep { get; set; }
        public int ReviewerCount { get; set; }

    }
}
