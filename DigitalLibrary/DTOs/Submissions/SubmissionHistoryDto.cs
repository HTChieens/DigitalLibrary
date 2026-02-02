namespace DigitalLibrary.DTOs.Submissions
{
    public class SubmissionHistoryDto
    {
        public Guid Id { get; set; }
        public string PerformedById { get; set; } = null!;
        public string PerformedByName { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
