using DigitalLibrary.DTOs.User;
using DigitalLibrary.Models;

namespace DigitalLibrary.Services
{
    public interface IAuthenService
    {
        //Task<bool> RegisterAsync(RegisterDto registerDTO);
        Task<LoginResponseDto> LoginAsync(LoginDto loginDTO);
    }
}
