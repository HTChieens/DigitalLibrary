namespace DigitalLibrary.DTOs.Documents
{
    public class UploadNewFileDto
    {
        public string FilePath { get; set; } = null!;
        public string? ChangeNote { get; set; }
        public Guid SubmissionId { get; set; }
    }
}
