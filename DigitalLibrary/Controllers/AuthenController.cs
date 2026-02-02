using DigitalLibrary.DTOs;
using DigitalLibrary.DTOs.User;
using DigitalLibrary.Repositories;
using DigitalLibrary.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto.Generators;
using System.Security.Claims;
using System.Security.Cryptography;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenController : ControllerBase
    {
        private readonly IAuthenService _authenService;
        private readonly IUserRepository _userRepo;
        private readonly IUserOtpCodeRepository _userOtpRepo;
        private readonly IEmailService _emailService;
        private readonly IPasswordHasherService  passwordHasher;
        private readonly IJwtTokenService jwtTokenService;
        public AuthenController(IAuthenService authenService, IUserRepository repo,
            IUserOtpCodeRepository userOtpRepo, IEmailService emailService, IPasswordHasherService passwordHasher,
            IJwtTokenService jwtTokenService)
        {
            this._authenService = authenService;
            this._userRepo = repo;
            _userOtpRepo = userOtpRepo;
            _emailService = emailService;
            this.passwordHasher = passwordHasher;
            this.jwtTokenService = jwtTokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login(LoginDto loginDto)
        {
            var result = await this._authenService.LoginAsync(loginDto);
            if (result == null)
            {
                return Unauthorized(new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "Email hoặc mật khẩu không chính xác"
                });
            }
            return Ok(new ApiResponse<LoginResponseDto>
            {
                Success = true,
                Message = "Đăng nhập thành công",
                Data = result
            });
        }
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp(SendOtpDto dto)
        {
            var user = await _userRepo.GetByEmail(dto.Email);

            if (user == null)
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Nếu email tồn tại, mã OTP đã được gửi",
                });
            var otp = GenerateOtp();
            string id = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                    .Replace("+", "")
                    .Replace("/", "")
                    .Substring(0, 20);
            string body = $@"
                Xin chào {user.Name},

                Mã xác thực (OTP) của bạn là: {otp}

                Mã này có hiệu lực trong {5} phút.
                Vui lòng không chia sẻ mã này với bất kỳ ai.

                Nếu bạn không yêu cầu mã này, hãy bỏ qua email.

                Trân trọng,
                Digital Library Team
                ";
            await _emailService.SendAsync(dto.Email, "Mã otp", body);
            await this._userOtpRepo.Add(new UserOtpCode
            {
                CreatedAt = DateTime.Now,
                ExpiredAt = DateTime.Now.AddMinutes(5),
                IsUsed = false,
                OtpCode = otp,
                UserId = user.ID
            });
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Nếu email tồn tại, mã OTP đã được gửi",
            });
        }
        [HttpPost("verify-otp")]
        public async Task<ActionResult<ApiResponse<object>>> VerifyOtp(VerifyOtpDto dto)
        {
            var user = await _userRepo.GetByEmail(dto.Email);
            if (user == null)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = false,
                    Message = "OTP không hợp lệ hoặc đã hết hạn"
                });
            }
            var userOtp = await _userOtpRepo.GetByUserId(user.ID);
            if (userOtp==null ||userOtp.OtpCode != dto.Otp ||
                userOtp.ExpiredAt < DateTime.Now||
                userOtp.IsUsed)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "OTP không hợp lệ hoặc đã hết hạn",

                });
            }
            userOtp.CreatedAt = DateTime.Now;
            userOtp.IsUsed = true;
            await _userRepo.Update(user);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Xác thực OTP thành công",
                Data = new
                {
                    AccessTokent = jwtTokenService.GenerateAccessToken(user)
                }
            });
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId== null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Vui lòng xác thực otp trước"
                });
            }
            var user = await _userRepo.Find(userId);
            if (user == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Vui lòng xác thực otp trước"
                });
            }
            user.PasswordHash =this.passwordHasher.HashPassword(dto.NewPassword);
            await _userRepo.Update(user);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Đổi mật khẩu thành công",
            });
        }

        public static string GenerateOtp()
        {
            byte[] bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);

            int value = BitConverter.ToInt32(bytes, 0);
            value = Math.Abs(value % 1_000_000);

            return value.ToString("D6"); 
        }
    }
}
