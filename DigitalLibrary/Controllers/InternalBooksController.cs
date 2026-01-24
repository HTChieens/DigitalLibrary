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
    public class InternalBooksController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public InternalBooksController(DigitalLibraryContext context)
        {
            _context = context;
        }

        // GET: api/InternalBooks
        [HttpGet]
        public async Task<ActionResult<object>> GetInternalBooks()
        {
            try
            {
                var internalBooks = await _context.InternalBooks
                    .Include(ib => ib.Document)
                    .Select(ib => new
                    {
                        ib.DocumentID,
                        ib.Faculty,
                        ib.DocumentType,
                        ib.Version,
                        Document = new
                        {
                            ib.Document.ID,
                            ib.Document.Title,
                            ib.Document.CreatedAt
                        }
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách giáo trình nội bộ thành công",
                    data = internalBooks,
                    count = internalBooks.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách giáo trình nội bộ",
                    error = ex.Message
                });
            }
        }

        // GET: api/InternalBooks/{documentId}
        [HttpGet("{documentId}")]
        public async Task<ActionResult<object>> GetInternalBook(string documentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(documentId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lấy dữ liệu thất bại",
                        error = "Document ID không được để trống"
                    });
                }

                var internalBook = await _context.InternalBooks
                    .Include(ib => ib.Document)
                    .Where(ib => ib.DocumentID == documentId)
                    .Select(ib => new
                    {
                        ib.DocumentID,
                        ib.Faculty,
                        ib.DocumentType,
                        ib.Version,
                        //Document = new
                        //{
                        //    ib.Document.ID,
                        //    ib.Document.Title,
                        //    ib.Document.CreatedAt,
                        //    ib.Document.UpdatedAt
                        //}
                    })
                    .FirstOrDefaultAsync();

                if (internalBook == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy giáo trình nội bộ",
                        error = $"Giáo trình nội bộ với Document ID {documentId} không tồn tại"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin giáo trình nội bộ thành công",
                    data = internalBook
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy thông tin giáo trình nội bộ",
                    error = ex.Message
                });
            }
        }

        // GET: api/InternalBooks/faculty/{faculty}
        [HttpGet("faculty/{faculty}")]
        public async Task<ActionResult<object>> GetInternalBooksByFaculty(string faculty)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(faculty))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lấy dữ liệu thất bại",
                        error = "Faculty không được để trống"
                    });
                }

                var internalBooks = await _context.InternalBooks
                    .Where(ib => ib.Faculty.Contains(faculty))
                    .Include(ib => ib.Document)
                    .Select(ib => new
                    {
                        ib.DocumentID,
                        ib.Faculty,
                        ib.DocumentType,
                        ib.Version,
                        Document = new
                        {
                            ib.Document.ID,
                            ib.Document.Title
                        }
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Lấy danh sách giáo trình của khoa '{faculty}' thành công",
                    data = internalBooks,
                    count = internalBooks.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách giáo trình theo khoa",
                    error = ex.Message
                });
            }
        }

        // GET: api/InternalBooks/type/{documentType}
        [HttpGet("type/{documentType}")]
        public async Task<ActionResult<object>> GetInternalBooksByType(string documentType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(documentType))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lấy dữ liệu thất bại",
                        error = "Document Type không được để trống"
                    });
                }

                var internalBooks = await _context.InternalBooks
                    .Where(ib => ib.DocumentType == documentType)
                    .Include(ib => ib.Document)
                    .Select(ib => new
                    {
                        ib.DocumentID,
                        ib.Faculty,
                        ib.DocumentType,
                        ib.Version,
                        Document = new
                        {
                            ib.Document.ID,
                            ib.Document.Title
                        }
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Lấy danh sách tài liệu loại '{documentType}' thành công",
                    data = internalBooks,
                    count = internalBooks.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách giáo trình theo loại",
                    error = ex.Message
                });
            }
        }

        // POST: api/InternalBooks
        [HttpPost]
        public async Task<ActionResult<object>> PostInternalBook(InternalBookCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.DocumentID))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo giáo trình nội bộ thất bại",
                        error = "Document ID không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.Faculty))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo giáo trình nội bộ thất bại",
                        error = "Faculty không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.DocumentType))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo giáo trình nội bộ thất bại",
                        error = "Document Type không được để trống"
                    });
                }

                if (dto.Version <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo giáo trình nội bộ thất bại",
                        error = "Version phải lớn hơn 0"
                    });
                }

                // Kiểm tra Document tồn tại
                var documentExists = await _context.Documents.AnyAsync(d => d.ID == dto.DocumentID);
                if (!documentExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Tạo giáo trình nội bộ thất bại",
                        error = $"Tài liệu với ID {dto.DocumentID} không tồn tại"
                    });
                }

                // Kiểm tra giáo trình đã tồn tại chưa
                var exists = await _context.InternalBooks.AnyAsync(ib => ib.DocumentID == dto.DocumentID);
                if (exists)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Tạo giáo trình nội bộ thất bại",
                        error = $"Giáo trình nội bộ với Document ID {dto.DocumentID} đã tồn tại"
                    });
                }

                var internalBook = new InternalBook
                {
                    DocumentID = dto.DocumentID,
                    Faculty = dto.Faculty.Trim(),
                    DocumentType = dto.DocumentType.Trim(),
                    Version = dto.Version
                };

                _context.InternalBooks.Add(internalBook);
                await _context.SaveChangesAsync();

                var result = await _context.InternalBooks
                    .Include(ib => ib.Document)
                    .Where(ib => ib.DocumentID == internalBook.DocumentID)
                    .Select(ib => new
                    {
                        ib.DocumentID,
                        ib.Faculty,
                        ib.DocumentType,
                        ib.Version,
                        Document = new
                        {
                            ib.Document.ID,
                            ib.Document.Title
                        }
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetInternalBook),
                    new { documentId = internalBook.DocumentID },
                    new
                    {
                        success = true,
                        message = "Tạo giáo trình nội bộ thành công",
                        data = result
                    });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lưu giáo trình nội bộ vào database",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi tạo giáo trình nội bộ",
                    error = ex.Message
                });
            }
        }

        // PUT: api/InternalBooks/{documentId}
        [HttpPut("{documentId}")]
        public async Task<ActionResult<object>> PutInternalBook(string documentId, InternalBookUpdateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(documentId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Document ID không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.Faculty))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Faculty không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.DocumentType))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Document Type không được để trống"
                    });
                }

                if (dto.Version <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Version phải lớn hơn 0"
                    });
                }

                var internalBook = await _context.InternalBooks.FindAsync(documentId);
                if (internalBook == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = $"Giáo trình nội bộ với Document ID {documentId} không tồn tại"
                    });
                }

                internalBook.Faculty = dto.Faculty.Trim();
                internalBook.DocumentType = dto.DocumentType.Trim();
                internalBook.Version = dto.Version;

                _context.Entry(internalBook).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var result = await _context.InternalBooks
                    .Include(ib => ib.Document)
                    .Where(ib => ib.DocumentID == documentId)
                    .Select(ib => new
                    {
                        ib.DocumentID,
                        ib.Faculty,
                        ib.DocumentType,
                        ib.Version,
                        Document = new
                        {
                            ib.Document.ID,
                            ib.Document.Title
                        }
                    })
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật giáo trình nội bộ thành công",
                    data = result
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.InternalBooks.AnyAsync(ib => ib.DocumentID == documentId))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Giáo trình nội bộ không tồn tại hoặc đã bị xóa"
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Xung đột dữ liệu. Giáo trình nội bộ có thể đã được cập nhật bởi người dùng khác"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi cập nhật giáo trình nội bộ",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/InternalBooks/{documentId}
        [HttpDelete("{documentId}")]
        public async Task<ActionResult<object>> DeleteInternalBook(string documentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(documentId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Xóa thất bại",
                        error = "Document ID không được để trống"
                    });
                }

                var internalBook = await _context.InternalBooks.FindAsync(documentId);

                if (internalBook == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Xóa thất bại",
                        error = $"Giáo trình nội bộ với Document ID {documentId} không tồn tại"
                    });
                }

                _context.InternalBooks.Remove(internalBook);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Xóa giáo trình nội bộ thành công",
                    deletedDocumentId = documentId
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Xóa thất bại",
                    error = "Không thể xóa giáo trình nội bộ. Có thể đang được sử dụng bởi dữ liệu khác",
                    details = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi xóa giáo trình nội bộ",
                    error = ex.Message
                });
            }
        }
    }
}