namespace DigitalLibrary.DTOs.Documents
{
    public class UploadNewFileDto
    {
        public Guid SubmissionId { get; set; }
        public string FilePath { get; set; } = null!;
        public string? ChangeNote { get; set; }
    }
}
