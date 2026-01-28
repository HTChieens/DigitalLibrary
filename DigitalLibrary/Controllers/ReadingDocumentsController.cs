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
        public async Task<ActionResult<ApiResponse<ReadingDocumentDto>>> Update([FromBody] ReadingDocumentDto dto)
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
            var response = new ReadingDocumentDto
            {
                DocumentID  = update.DocumentID,
                CurrentPage = update.CurrentPage,
                FirstReadAt = update.FirstReadAt,
                LastReadAt = update.LastReadAt,
                IsCounted = update.IsCounted
            };
            return Ok(new ApiResponse<ReadingDocumentDto>
            {
                Success = true,
                Message = "Cập nhật thành công",
                Data = response
            });
        }
    }
}
