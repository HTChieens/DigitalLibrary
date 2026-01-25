using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.DTOs.User
{
    public class LoginResultDto
    {
        public LoginResponseDto loginResponse { get; set; } = null;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
