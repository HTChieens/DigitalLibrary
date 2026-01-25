using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.DTOs.User
{
    public class UserUpdateDto
    {
        public string Name { get; set; } = null!;
        public string? PhoneNumber { get; set; }

    }

}
