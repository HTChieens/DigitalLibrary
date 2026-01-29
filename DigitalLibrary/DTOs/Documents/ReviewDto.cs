namespace DigitalLibrary.DTOs.Documents
{
    public class ReviewDto
    {
        public string Id { get; set; }
        public string DocumentId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; } // Cực kỳ quan trọng để hiển thị
        public int? Rating { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
