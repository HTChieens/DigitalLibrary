using DigitalLibrary.DTOs;
using DigitalLibrary.DTOs.Authors;
using DigitalLibrary.DTOs.User;
using DigitalLibrary.Models;
using DigitalLibrary.Repositories;
using DigitalLibrary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IPasswordHasherService _passwordHasher;
        private readonly IUserRepository _userRepo;
        public UsersController(IUserRepository userRepo, IPasswordHasherService passwordHasher)
        {
            _userRepo = userRepo;
            _passwordHasher = passwordHasher;
        }
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> MyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await this._userRepo.Find(userId);
            if (user == null) {
                return NotFound(new ApiResponse<UserProfileDto>
                {
                    Success = false,
                    Message = "Không tìm thấy user",
                    Data = null
                }); 
            }
            var profile = new UserProfileDto
            {
                Name = user.Name,
                Email = user.Email,
                Class = user.Class, 
                PhoneNumber = user.PhoneNumber,
            };
            return Ok(new ApiResponse<UserProfileDto>
            {
                Success = true,
                Message ="Lấy thông tin cá nhân thành công",
                Data = profile
            });
        }
        [HttpGet("me/author")]
        public async Task<ActionResult<ApiResponse<AuthorDto>>> Author()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Không tìm thấy user"

            });
            var author = await this._userRepo.GetAuthor(userId);
            if (author == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Bạn không phải là một tác giả",
                });
            }
            var profile = new AuthorDto
            {
                Name = author.Name,
                Email = author.Email,
                Description = author.Description,
                Expertise = author.Expertise,
                Image = author.Image   
            };
            return Ok(new ApiResponse<AuthorDto>
            {
                Success = true,
                Message = "Lấy thông tin tác giả thành công",
                Data = profile
            });
        }
        [HttpPut("me")]
        public async Task<ActionResult<ApiResponse<UserUpdateDto>>> Update([FromBody] UserUpdateDto user)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userUpdate = await _userRepo.Find(id);
            if (userUpdate == null)
            {
                return NotFound(new ApiResponse<User>
                {
                    Success = false,
                    Message = "Không tìm thấy user",
                    Data = null
                });
            }
            userUpdate.PhoneNumber = user.PhoneNumber;
            userUpdate.Name = user.Name;
            userUpdate.UpdatedAt = DateTime.Now;
            await this._userRepo.Update(userUpdate);
            var response = new UserUpdateDto
            {
       
                Name = userUpdate.Name,
                PhoneNumber = userUpdate.PhoneNumber,
            };
            return Ok(new ApiResponse<UserUpdateDto>
            {
                Success = true,
                Message = "Cập nhật thành công",
                Data = response
            });
        }
        [HttpPut("change-password")]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword(ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userRepo.Find(userId);
            if (user == null)
            {
                return  Unauthorized();
            }
            if(!this._passwordHasher.VerifyPassword(dto.OldPassword,user.PasswordHash))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Mật khẩu không đúng"
                });
            }
            if (dto.OldPassword == dto.NewPassword)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Mật khẩu mới không được trùng mật khẩu cũ"
                });
            }
            user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.Now;
            await this._userRepo.Update(user);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Thay đổi mật khẩu thành công"
            });
        }
    }
}
