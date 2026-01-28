using DigitalLibrary.DTOs;
using DigitalLibrary.DTOs.User;
using DigitalLibrary.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenController : ControllerBase
    {
        private readonly IAuthenService _authenService;
        public AuthenController(IAuthenService authenService)
        {
            this._authenService = authenService;
        }
     
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
        {
            var result = await this._authenService.LoginAsync(loginDto);
            if ( result  == null)
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
    }
}
