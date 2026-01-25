using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.DTOs.Authors
{
    public class AuthorDto
    {
        public string Name { get; set; } = null!;

        public string? Email { get; set; }

        public string? Description { get; set; }

        public string? Image { get; set; }

        public string? Expertise { get; set; }
    }

}
