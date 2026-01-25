using DigitalLibrary.Data;
using DigitalLibrary.DTOs;
using DigitalLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadsController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public DownloadsController(DigitalLibraryContext context)
        {
            _context = context;
        }

        // GET: api/Downloads?id=1&userId=abc&documentId=xyz&fromDate=2024-01-01&toDate=2024-12-31
        [HttpGet]
        public async Task<ActionResult<object>> GetDownloads(
            [FromQuery] long? id,
            [FromQuery] string? userId,
            [FromQuery] string? documentId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var query = _context.Downloads
                    .Include(d => d.User)
                    .Include(d => d.Document)
                    .AsQueryable();

                // Filter theo ID
                if (id.HasValue)
                {
                    query = query.Where(d => d.ID == id.Value);
                }

                // Filter theo UserID
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    query = query.Where(d => d.UserID == userId);
                }

                // Filter theo DocumentID
                if (!string.IsNullOrWhiteSpace(documentId))
                {
                    query = query.Where(d => d.DocumentID == documentId);
                }

                // Filter theo khoảng thời gian
                if (fromDate.HasValue)
                {
                    query = query.Where(d => d.DownloadedAt >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(d => d.DownloadedAt <= toDate.Value);
                }

                var downloads = await query
                    .OrderByDescending(d => d.DownloadedAt)
                    .Select(d => new
                    {
                        d.ID,
                        d.UserID,
                        d.DocumentID,
                        d.DownloadedAt,
                        //User = new
                        //{
                        //    d.User.ID,
                        //    d.User.Username,
                        //    d.User.Email
                        //},
                        //Document = new
                        //{
                        //    d.Document.ID,
                        //    d.Document.Title,
                        //    d.Document.DocumentType
                        //}
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách lượt tải xuống thành công",
                    filters = new
                    {
                        id,
                        userId,
                        documentId,
                        fromDate,
                        toDate
                    },
                    data = downloads,
                    count = downloads.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách lượt tải xuống",
                    error = ex.Message
                });
            }
        }

        

        // GET: api/Downloads/count?userId=abc&documentId=xyz
        [HttpGet("count")]
        public async Task<ActionResult<object>> GetDownloadCount(
            [FromQuery] string? userId,
            [FromQuery] string? documentId)
        {
            try
            {
                var query = _context.Downloads.AsQueryable();

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    query = query.Where(d => d.UserID == userId);
                }

                if (!string.IsNullOrWhiteSpace(documentId))
                {
                    query = query.Where(d => d.DocumentID == documentId);
                }

                var count = await query.CountAsync();

                return Ok(new
                {
                    success = true,
                    message = "Đếm số lượt tải xuống thành công",
                    filters = new
                    {
                        userId,
                        documentId
                    },
                    count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi đếm số lượt tải xuống",
                    error = ex.Message
                });
            }
        }

        // POST: api/Downloads
        [HttpPost]
        public async Task<ActionResult<object>> Download(DownloadCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.UserID))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tải xuống thất bại",
                        error = "User ID không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.DocumentID))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tải xuống thất bại",
                        error = "Document ID không được để trống"
                    });
                }

                // Kiểm tra User tồn tại
                var userExists = await _context.Users.AnyAsync(u => u.ID == dto.UserID);
                if (!userExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Tải xuống thất bại",
                        error = $"Người dùng với ID {dto.UserID} không tồn tại"
                    });
                }

                // Kiểm tra Document tồn tại
                var documentExists = await _context.Documents.AnyAsync(d => d.ID == dto.DocumentID);
                if (!documentExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Tải xuống thất bại",
                        error = $"Tài liệu với ID {dto.DocumentID} không tồn tại"
                    });
                }

                // Kiểm tra Document có bị xóa không
                var document = await _context.Documents.FindAsync(dto.DocumentID);
                if (document != null && document.IsDeleted)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tải xuống thất bại",
                        error = "Tài liệu đã bị xóa, không thể tải xuống"
                    });
                }

                var download = new Download
                {
                    UserID = dto.UserID,
                    DocumentID = dto.DocumentID,
                    DownloadedAt = DateTime.UtcNow
                };

                _context.Downloads.Add(download);
                await _context.SaveChangesAsync();

                var result = await _context.Downloads
                    .Include(d => d.User)
                    .Include(d => d.Document)
                    .Where(d => d.ID == download.ID)
                    .Select(d => new
                    {
                        d.ID,
                        d.UserID,
                        d.DocumentID,
                        d.DownloadedAt,
                        //User = new
                        //{
                        //    d.User.ID,
                        //    d.User.Username,
                        //    d.User.Email
                        //},
                        //Document = new
                        //{
                        //    d.Document.ID,
                        //    d.Document.Title,
                        //    d.Document.DocumentType,
                        //    d.Document.FilePath
                        //}
                    })
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    success = true,
                    message = "Ghi nhận tải xuống thành công",
                    data = result
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lưu thông tin tải xuống vào database",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi ghi nhận tải xuống",
                    error = ex.Message
                });
            }
        }
    }
}