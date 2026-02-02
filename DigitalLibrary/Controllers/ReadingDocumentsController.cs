using DigitalLibrary.DTOs;
using DigitalLibrary.DTOs.ReadingDocuments;
using DigitalLibrary.Models;
using DigitalLibrary.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Security.Claims;


namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    [Authorize]
    public class ReadingDocumentsController : ControllerBase
    {
        private readonly IReadingDocumentRepository _repo;
        public ReadingDocumentsController(IReadingDocumentRepository repo)
        {
            _repo = repo;
        }
       
        [HttpPut("")]
        public async Task<ActionResult<ApiResponse<RdResponseDto>>> Update([FromBody] RdAddDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var update = await _repo.Find(userId,dto.DocumentID);
            if (update == null)
            {
                try
                {
                    await _repo.Add(new ReadingDocument
                    {
                        UserID = userId,
                        DocumentID = dto.DocumentID,
                        CurrentPage = dto.CurrentPage,
                        FirstReadAt = DateTime.Now,
                        LastReadAt = DateTime.Now,
                        IsCounted = true,
                    });
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Thêm mới lượt đọc",
                    });
                }
                catch (Exception)
                {
                    return NotFound( new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Sách hoặc user không tồn tại",
                    });
                }
            }
            update.CurrentPage = dto.CurrentPage;
            update.LastReadAt= DateTime.Now;
            await this._repo.Update(update);
            var response = new RdResponseDto
            {
                DocumentId  = update.DocumentID,
                CurrentPage = update.CurrentPage,
                FirstReadAt = update.FirstReadAt,
                LastReadAt = update.LastReadAt,
                //IsCounted = update.IsCounted
            };
            return Ok(new ApiResponse<RdResponseDto>
            {
                Success = true,
                Message = "Cập nhật thành công",
                Data = response
            });
        }
        [HttpPost("")]
        public async Task<ActionResult<ApiResponse<bool>>> Add([FromBody] RdAddDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _repo.Add(new ReadingDocument
            {
                UserID = userId,
                DocumentID = dto.DocumentID,
                CurrentPage = dto.CurrentPage,
                FirstReadAt = DateTime.Now,
                LastReadAt = DateTime.Now,
                IsCounted = true,
            });
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Thêm mới lượt đọc",
            });
        }
        [HttpGet("")]
        public async Task<ActionResult<ApiResponse<List<RdResponseDto>>>> Reading()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return BadRequest();
            }
            var result = await _repo.GetByUserId(userId);
            return Ok(new ApiResponse<List<RdResponseDto>>
            {
                Success = true,
                Message = "lấy danh sách thành công",
                Data = result
            });
        }
    }
}
