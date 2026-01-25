
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using DigitalLibrary.DTOs;
using DigitalLibrary.DTOs.Authors;
using DigitalLibrary.Repositories;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
 
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _repo;
        public AuthorsController(IAuthorRepository repo)
        {
            _repo = repo;
        }
        
        // GET: api/Users
        [HttpGet]
        
        public async Task<ActionResult<ApiResponse<ICollection<AuthorDto>>>> GetAuthors()
        {
            var results = await _repo.GetAllAsync();
            var profiles = new List<AuthorDto>();
            foreach (var result in results)
            {
                profiles.Add(new AuthorDto { Name = result.Name,Email = result.Email,Description=result.Description,Expertise = result.Expertise,Image = result.Image});

            }
            return Ok( new ApiResponse<ICollection<AuthorDto>>
            {
                Success = true,
                Message = "Lấy danh sách author thành công",
                Data = profiles
            });
        } // GET: api/Users
        [HttpGet("{authorId}/documents")]
        public async Task<ActionResult<ApiResponse<ICollection<Document>>>> GetDocuments(string authorId)
        {
            var results = await _repo.GetDocuments(authorId);
            return Ok(new ApiResponse<ICollection<Document>>
            {
                Success = true,
                Message = "Lấy danh sách Document thành công",
                Data = results
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<AuthorDto>>> Update(string id, AuthorDto dto)
        {
            var update = await _repo.Find(id);
            if (update == null)
            {
                return NotFound(new ApiResponse<AuthorDto>
                {
                    Success = false,
                    Message = "Không tìm thấy author",
                });
            }
            update.Name = dto.Name;
            update.Email = dto.Email;
            update.Description = dto.Description;
            update.Image = dto.Image;
            update.Expertise = dto.Expertise;   
            await this._repo.Update(update);
            var response = new AuthorDto
            {
                Name = update.Name,
                Email = update.Email,
                Description = update.Description,
                Expertise = update.Expertise,
                Image = update.Image
            };
            return Ok(new ApiResponse<AuthorDto>
            {
                Success = true,
                Message = "Cập nhật thành công",
                Data = response
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<AuthorDto>>> Add([FromBody] AuthorDto dto)
        {
            var maxId = await _repo.GetMaxId();
            var author= new Author
            {
                ID = (maxId + 1).ToString(),
                Name = dto.Name,
                Email = dto.Email,
                Description = dto.Description,
                Expertise = dto.Expertise,
                Image = dto.Image                
            };
            await this._repo.Add(author);
            var response = new AuthorDto
            {
                Name = author.Name,
                Description = author.Description,
                Expertise = author.Expertise,
                Image = author.Image,
                Email = author.Email,
            };
            return Ok(new ApiResponse<AuthorDto>
            {
                Success = true,
                Message = "Thêm thành công",
                Data = response
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<AuthorDto>>> Delete(string id)
        {
            var result = await _repo.Find(id);
            if (result == null)
            {
                return NotFound(new ApiResponse<AuthorDto>
                {
                    Success = false,
                    Message = "Không tìm thấy author",
                });
            }
            var response = new AuthorDto
            {
              Name = result.Name
              , Description = result.Description,
              Expertise = result.Expertise,
              Email = result.Email,
              Image  = result.Image 
            };
            try
            {
                await this._repo.Delete(result);
                return Ok(new ApiResponse<AuthorDto>
                {
                    Success = true,
                    Message = "Xóa thành công",
                    Data = response
                });
            }
            catch (DbUpdateException)
            {
                return Conflict(new ApiResponse<AuthorDto>
                {
                    Success = false,
                    Message = "Không thể xóa author vì còn dữ liệu liên quan",
                    Data = response
                });
            } 
        }
    }
}
