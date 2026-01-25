using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.DTOs.User
{
    public class UserProfileDto
    {

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public string? Class { get; set; }
    }

}
