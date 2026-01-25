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
    public class SavedDocumentsController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public SavedDocumentsController(DigitalLibraryContext context)
        {
            _context = context;
        }

        // GET: api/SavedDocuments?userId=abc&documentId=xyz&fromDate=2024-01-01&toDate=2024-12-31
        [HttpGet]
        public async Task<ActionResult<object>> GetSavedDocuments(
            [FromQuery] string? userId,
            [FromQuery] string? documentId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? documentType)
        {
            try
            {
                var query = _context.SavedDocuments
                    .Include(sd => sd.User)
                    .Include(sd => sd.Document)
                    .AsQueryable();

                // Filter theo UserID
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    query = query.Where(sd => sd.UserID == userId);
                }

                // Filter theo DocumentID
                if (!string.IsNullOrWhiteSpace(documentId))
                {
                    query = query.Where(sd => sd.DocumentID == documentId);
                }

                // Filter theo khoảng thời gian lưu
                if (fromDate.HasValue)
                {
                    query = query.Where(sd => sd.SavedAt >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(sd => sd.SavedAt <= toDate.Value);
                }

                // Filter theo loại tài liệu
                if (!string.IsNullOrWhiteSpace(documentType))
                {
                    query = query.Where(sd => sd.Document.DocumentType == documentType);
                }


                var savedDocuments = await query
                    .OrderByDescending(sd => sd.SavedAt)
                    .Select(sd => new
                    {
                        sd.UserID,
                        sd.DocumentID,
                        sd.SavedAt,
                        //User = new
                        //{
                        //    sd.User.ID,
                        //    sd.User.Username,
                        //    sd.User.Email
                        //},
                        //Document = new
                        //{
                        //    sd.Document.ID,
                        //    sd.Document.Title,
                        //    sd.Document.Description,
                        //    sd.Document.DocumentType,
                        //    sd.Document.CoverPath,
                        //    sd.Document.PublicationDate,
                        //    sd.Document.PageNum,
                        //    sd.Document.IsDeleted
                        //}
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách tài liệu đã lưu thành công",
                    filters = new
                    {
                        userId,
                        documentId,
                        fromDate,
                        toDate,
                        documentType
                    },
                    data = savedDocuments,
                    count = savedDocuments.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách tài liệu đã lưu",
                    error = ex.Message
                });
            }
        }

        // POST: api/SavedDocuments/save
        [HttpPost("save")]
        public async Task<ActionResult<object>> SaveDoc(SavedDocumentCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.UserID))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lưu tài liệu thất bại",
                        error = "User ID không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.DocumentID))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lưu tài liệu thất bại",
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
                        message = "Lưu tài liệu thất bại",
                        error = $"Người dùng với ID {dto.UserID} không tồn tại"
                    });
                }

                // Kiểm tra Document tồn tại
                var document = await _context.Documents.FindAsync(dto.DocumentID);
                if (document == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Lưu tài liệu thất bại",
                        error = $"Tài liệu với ID {dto.DocumentID} không tồn tại"
                    });
                }

                // Kiểm tra Document có bị xóa không
                if (document.IsDeleted)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lưu tài liệu thất bại",
                        error = "Tài liệu đã bị xóa, không thể lưu"
                    });
                }

                // Kiểm tra đã lưu chưa
                var exists = await _context.SavedDocuments
                    .AnyAsync(sd => sd.UserID == dto.UserID && sd.DocumentID == dto.DocumentID);
                if (exists)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Lưu tài liệu thất bại",
                        error = "Tài liệu này đã được lưu trước đó"
                    });
                }

                var savedDocument = new SavedDocument
                {
                    UserID = dto.UserID,
                    DocumentID = dto.DocumentID,
                    SavedAt = DateTime.UtcNow
                };

                _context.SavedDocuments.Add(savedDocument);
                await _context.SaveChangesAsync();

                var result = await _context.SavedDocuments
                    .Include(sd => sd.User)
                    .Include(sd => sd.Document)
                    .Where(sd => sd.UserID == savedDocument.UserID && sd.DocumentID == savedDocument.DocumentID)
                    .Select(sd => new
                    {
                        sd.UserID,
                        sd.DocumentID,
                        sd.SavedAt,
                        //User = new
                        //{
                        //    sd.User.ID,
                        //    sd.User.Username,
                        //    sd.User.Email
                        //},
                        //Document = new
                        //{
                        //    sd.Document.ID,
                        //    sd.Document.Title,
                        //    sd.Document.Description,
                        //    sd.Document.DocumentType,
                        //    sd.Document.CoverPath
                        //}
                    })
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lưu tài liệu thành công",
                    data = result
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lưu tài liệu vào database",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lưu tài liệu",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/SavedDocuments/unsave?userId=abc&documentId=xyz
        [HttpDelete("unsave")]
        public async Task<ActionResult<object>> Unsave(
            [FromQuery] string userId,
            [FromQuery] string documentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Bỏ lưu thất bại",
                        error = "User ID không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(documentId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Bỏ lưu thất bại",
                        error = "Document ID không được để trống"
                    });
                }

                var savedDocument = await _context.SavedDocuments
                    .FirstOrDefaultAsync(sd => sd.UserID == userId && sd.DocumentID == documentId);

                if (savedDocument == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Bỏ lưu thất bại",
                        error = "Không tìm thấy tài liệu đã lưu"
                    });
                }

                _context.SavedDocuments.Remove(savedDocument);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Bỏ lưu tài liệu thành công",
                    userId,
                    documentId
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Bỏ lưu thất bại",
                    error = "Không thể bỏ lưu tài liệu",
                    details = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi bỏ lưu tài liệu",
                    error = ex.Message
                });
            }
        }
    }
}