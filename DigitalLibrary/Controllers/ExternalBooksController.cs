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
    public class ExternalBooksController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public ExternalBooksController(DigitalLibraryContext context)
        {
            _context = context;
        }

        // GET: api/ExternalBooks
        [HttpGet]
        public async Task<ActionResult<object>> GetExternalBooks()
        {
            try
            {
                var externalBooks = await _context.ExternalBooks
                    .Include(eb => eb.Document)
                    .Select(eb => new
                    {
                        eb.DocumentID,
                        eb.Publisher,
                        eb.Version,
                        //Document = new
                        //{
                        //    eb.Document.ID,
                        //    eb.Document.Title,
                        //    eb.Document.CreatedAt
                        //}
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách sách ngoại thành công",
                    data = externalBooks,
                    count = externalBooks.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách sách ngoại",
                    error = ex.Message
                });
            }
        }

        // GET: api/ExternalBooks/{documentId}
        [HttpGet("{documentId}")]
        public async Task<ActionResult<object>> GetExternalBook(string documentId)
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

                var externalBook = await _context.ExternalBooks
                    .Include(eb => eb.Document)
                    .Where(eb => eb.DocumentID == documentId)
                    .Select(eb => new
                    {
                        eb.DocumentID,
                        eb.Publisher,
                        eb.Version,
                        //Document = new
                        //{
                        //    eb.Document.ID,
                        //    eb.Document.Title,
                        //    eb.Document.CreatedAt,
                        //    eb.Document.UpdatedAt
                        //}
                    })
                    .FirstOrDefaultAsync();

                if (externalBook == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy sách ngoại",
                        error = $"Sách ngoại với Document ID {documentId} không tồn tại"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin sách ngoại thành công",
                    data = externalBook
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy thông tin sách ngoại",
                    error = ex.Message
                });
            }
        }

        // GET: api/ExternalBooks/publisher/{publisher}
        [HttpGet("publisher/{publisher}")]
        public async Task<ActionResult<object>> GetExternalBooksByPublisher(string publisher)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(publisher))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lấy dữ liệu thất bại",
                        error = "Publisher không được để trống"
                    });
                }

                var externalBooks = await _context.ExternalBooks
                    .Where(eb => eb.Publisher.Contains(publisher))
                    .Include(eb => eb.Document)
                    .Select(eb => new
                    {
                        eb.DocumentID,
                        eb.Publisher,
                        eb.Version,
                        Document = new
                        {
                            eb.Document.DocumentId,
                            eb.Document.Title
                        }
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Lấy danh sách sách của nhà xuất bản '{publisher}' thành công",
                    data = externalBooks,
                    count = externalBooks.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách sách theo nhà xuất bản",
                    error = ex.Message
                });
            }
        }

        // POST: api/ExternalBooks
        [HttpPost]
        public async Task<ActionResult<object>> PostExternalBook(ExternalBookCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.DocumentID))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo sách ngoại thất bại",
                        error = "Document ID không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.Publisher))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo sách ngoại thất bại",
                        error = "Publisher không được để trống"
                    });
                }

                if (dto.Version <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo sách ngoại thất bại",
                        error = "Version phải lớn hơn 0"
                    });
                }

                // Kiểm tra Document tồn tại
                var documentExists = await _context.Documents.AnyAsync(d => d.DocumentId == dto.DocumentID);
                if (!documentExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Tạo sách ngoại thất bại",
                        error = $"Tài liệu với ID {dto.DocumentID} không tồn tại"
                    });
                }

                // Kiểm tra sách ngoại đã tồn tại chưa
                var exists = await _context.ExternalBooks.AnyAsync(eb => eb.DocumentID == dto.DocumentID);
                if (exists)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Tạo sách ngoại thất bại",
                        error = $"Sách ngoại với Document ID {dto.DocumentID} đã tồn tại"
                    });
                }

                var externalBook = new ExternalBook
                {
                    DocumentID = dto.DocumentID,
                    Publisher = dto.Publisher.Trim(),
                    Version = dto.Version
                };

                _context.ExternalBooks.Add(externalBook);
                await _context.SaveChangesAsync();

                var result = await _context.ExternalBooks
                    .Include(eb => eb.Document)
                    .Where(eb => eb.DocumentID == externalBook.DocumentID)
                    .Select(eb => new
                    {
                        eb.DocumentID,
                        eb.Publisher,
                        eb.Version,
                        Document = new
                        {
                            eb.Document.DocumentId,
                            eb.Document.Title
                        }
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetExternalBook),
                    new { documentId = externalBook.DocumentID },
                    new
                    {
                        success = true,
                        message = "Tạo sách ngoại thành công",
                        data = result
                    });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lưu sách ngoại vào database",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi tạo sách ngoại",
                    error = ex.Message
                });
            }
        }

        // PUT: api/ExternalBooks/{documentId}
        [HttpPut("{documentId}")]
        public async Task<ActionResult<object>> PutExternalBook(string documentId, ExternalBookUpdateDto dto)
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

                if (string.IsNullOrWhiteSpace(dto.Publisher))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Publisher không được để trống"
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

                var externalBook = await _context.ExternalBooks.FindAsync(documentId);
                if (externalBook == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = $"Sách ngoại với Document ID {documentId} không tồn tại"
                    });
                }

                externalBook.Publisher = dto.Publisher.Trim();
                externalBook.Version = dto.Version;

                _context.Entry(externalBook).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var result = await _context.ExternalBooks
                    .Include(eb => eb.Document)
                    .Where(eb => eb.DocumentID == documentId)
                    .Select(eb => new
                    {
                        eb.DocumentID,
                        eb.Publisher,
                        eb.Version,
                        Document = new
                        {
                            eb.Document.DocumentId,
                            eb.Document.Title
                        }
                    })
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật sách ngoại thành công",
                    data = result
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.ExternalBooks.AnyAsync(eb => eb.DocumentID == documentId))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Sách ngoại không tồn tại hoặc đã bị xóa"
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Xung đột dữ liệu. Sách ngoại có thể đã được cập nhật bởi người dùng khác"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi cập nhật sách ngoại",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/ExternalBooks/{documentId}
        [HttpDelete("{documentId}")]
        public async Task<ActionResult<object>> DeleteExternalBook(string documentId)
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

                var externalBook = await _context.ExternalBooks.FindAsync(documentId);

                if (externalBook == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Xóa thất bại",
                        error = $"Sách ngoại với Document ID {documentId} không tồn tại"
                    });
                }

                _context.ExternalBooks.Remove(externalBook);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Xóa sách ngoại thành công",
                    deletedDocumentId = documentId
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Xóa thất bại",
                    error = "Không thể xóa sách ngoại. Có thể đang được sử dụng bởi dữ liệu khác",
                    details = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi xóa sách ngoại",
                    error = ex.Message
                });
            }
        }
    }
}