namespace DigitalLibrary.DTOs.Documents
{
    public class AuthorInputDto
    {
        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string? Expertise { get; set; }

        public string? Orcid { get; set; }
    }
}
