using DigitalLibrary.DTOs.User;
using DigitalLibrary.Models;

namespace DigitalLibrary.Services
{
    public interface IAuthenService
    {
        
        Task<LoginResponseDto> LoginAsync(LoginDto loginDTO);
        Task<string> ForgotPassword(SendOtpDto dto);

    }
}
