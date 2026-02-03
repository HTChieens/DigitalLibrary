namespace DigitalLibrary.DTOs.Documents
{
    public class ReviewDto
    {
        public string Id { get; set; }
        public string DocumentId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public byte? Rating { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsCurrentUser { get; set; }  // Thêm field này
    }
}
