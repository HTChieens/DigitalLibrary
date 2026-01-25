using DigitalLibrary.Data;
using DigitalLibrary.DTOs.User;
using DigitalLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibrary.Services
{
    public class AuthenService : IAuthenService
    {
        private readonly DigitalLibraryContext _context;
        private readonly IPasswordHasherService _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        public AuthenService(DigitalLibraryContext context,
            IPasswordHasherService passwordHasher,
            IJwtTokenService jwtTokenService)
        {
            _context = context;
            this._passwordHasher = passwordHasher;
            this._jwtTokenService = jwtTokenService;
        }
        public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDTO)
        {
            var user =await _context.Users.Include(u=>u.Role).FirstOrDefaultAsync(u => u.Email == loginDTO.Email);
            if (user == null)
            {
                return null ;
            }
            if(!this._passwordHasher.VerifyPassword(loginDTO.Password,user.PasswordHash))
            {
                return null;
            }
            var response = new LoginResponseDto
            {
                Email = user.Email,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Class = user.Class,
                AccessToken = this._jwtTokenService.GenerateAccessToken(user)
            };
            return response;
        }
    }
}
