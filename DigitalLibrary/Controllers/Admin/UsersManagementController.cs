using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using DigitalLibrary.Repositories;
using DigitalLibrary.DTOs.User;
using DigitalLibrary.DTOs;
using DigitalLibrary.Services;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    [Authorize(Roles = "Admin,Librarian")]
    public class UsersManagementController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IPasswordHasherService _passwordHasher;

        public UsersManagementController(IUserRepository userRepo,IPasswordHasherService passwordHasher)
        {
            _userRepo = userRepo;
            _passwordHasher = passwordHasher;
        }
        
        // GET: api/Users
        [HttpGet]
        
        public async Task<ActionResult<ApiResponse<ICollection<UserProfileDto>>>> GetUsers()
        {
            var users = await _userRepo.GetAllAsync();
            var userProfiles = new List<UserProfileDto>();
            foreach (var user in users) {
                userProfiles.Add(new UserProfileDto { Name = user.Name,Class = user.Class,Email = user.Email,PhoneNumber  = user.PhoneNumber });
            }
            return Ok( new ApiResponse<ICollection<UserProfileDto>>
            {
                Success = true,
                Message = "Lấy danh sách user thành công",
                Data = userProfiles
            });
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
      
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> Profile(string id)
        {
            var user = await this._userRepo.Find(id);
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
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<UserUpdateDto>>> Update(string id, UserUpdateDto user)
        {
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
        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserAddDto>>> Add([FromBody] UserAddDto userAddDto)
        {
            var user = new User
            {
                ID = Guid.NewGuid().ToString("N"),
                Name = userAddDto.Name,
                Email = userAddDto.Email,
                PasswordHash = this._passwordHasher.HashPassword(userAddDto.Password),
                Class = userAddDto.Class,
                PhoneNumber = userAddDto.Phone,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                RoleID = userAddDto.RoldId,
            };
            await this._userRepo.Add(user);
            var response = new UserAddDto
            {
                Name = user.Name,
                Email = userAddDto.Email,
                Class = userAddDto.Class,
                Phone = userAddDto.Phone,
                RoldId = userAddDto.RoldId,
            };
            return Ok(new ApiResponse<UserAddDto>
            {
                Success = true,
                Message = "Thêm thành công",
                Data = response
            });
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<UserDeleteDto>>> Delete(string id)
        {
            var user = await _userRepo.Find(id);
            if (user == null)
            {
                return NotFound(new ApiResponse<UserDeleteDto>
                {
                    Success = false,
                    Message = "Không tìm thấy user",
                    Data = null
                });
            }
            var response = new UserDeleteDto
            {
                Id = user.ID
            };
            try
            {
                await this._userRepo.Delete(user);
                return Ok(new ApiResponse<UserDeleteDto>
                {
                    Success = true,
                    Message = "Xóa thành công",
                    Data = response
                });
            }
            catch (DbUpdateException)
            {
                return Conflict(new ApiResponse<UserDeleteDto>
                {
                    Success = false,
                    Message = "Không thể xóa user vì còn dữ liệu liên quan",
                    Data = response
                });
            } 
        }
    }
}
