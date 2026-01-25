using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.DTOs.User
{
    public class LoginResponseDto
    {
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string Class{ get; set; } = string.Empty;
        public string PhoneNumber{ get; set; } = string.Empty;
        public string AccessToken{ get; set; } = string.Empty;
    }
}
