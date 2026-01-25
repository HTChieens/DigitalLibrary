using DigitalLibrary.DTOs;
using DigitalLibrary.Models;

namespace DigitalLibrary.Services
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken(User user);
    }
}
