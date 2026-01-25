
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using DigitalLibrary.DTOs;
using DigitalLibrary.DTOs.Authors;
using DigitalLibrary.DTOs.UserAuthors;
using DigitalLibrary.Repositories;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    [Authorize(Roles = "Admin")]
    public class UserAuthorsController : ControllerBase
    {
        private readonly IUserAuthorRepository _repo;
        public UserAuthorsController(IUserAuthorRepository repo)
        {
            _repo = repo;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserAuthorDto>>> Add([FromBody] UserAuthorDto dto)
        {
            var author= new User_Author
            {
               UserID = dto.UserId,
               AuthorID = dto.AuthorId,
            };
            await this._repo.Add(author);
            var response = new UserAuthorDto
            {
                UserId = dto.UserId,
                AuthorId = dto.AuthorId,
            };
            return Ok(new ApiResponse<UserAuthorDto>
            {
                Success = true,
                Message = "Thêm thành công",
                Data = response
            });
        }
        [HttpDelete()]
        public async Task<ActionResult<ApiResponse<UserAuthorDto>>> Delete(UserAuthorDto dto)
        {
            var result = await _repo.Find(dto);
            if (result == null)
            {
                return NotFound(new ApiResponse<AuthorDto>
                {
                    Success = false,
                    Message = "Không tìm thấy user_author",
                });
            }
            var response = new UserAuthorDto
            {
                UserId = result.UserID,
                AuthorId = result.AuthorID
            };
            try
            {
                await this._repo.Delete(result);
                return Ok(new ApiResponse<UserAuthorDto>
                {
                    Success = true,
                    Message = "Xóa thành công",
                    Data = response
                });
            }
            catch (DbUpdateException)
            {
                return Conflict(new ApiResponse<UserAuthorDto>
                {
                    Success = false,
                    Message = "Không thể xóa user_author vì còn dữ liệu liên quan",
                    Data = response
                });
            } 
        }
    }
}
