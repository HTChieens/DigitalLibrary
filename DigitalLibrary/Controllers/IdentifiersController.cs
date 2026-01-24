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
    public class IdentifiersController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public IdentifiersController(DigitalLibraryContext context)
        {
            _context = context;
        }

        // GET: api/Identifiers
        [HttpGet]
        public async Task<ActionResult<object>> GetIdentifiers()
        {
            try
            {
                var identifiers = await _context.Identifiers
                    .Include(i => i.Document)
                    .Select(i => new
                    {
                        i.ID,
                        i.DocumentID,
                        i.Type,
                        i.Value,
                        //Document = new
                        //{
                        //    i.Document.ID,
                        //    i.Document.Title
                        //}
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách định danh thành công",
                    data = identifiers,
                    count = identifiers.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách định danh",
                    error = ex.Message
                });
            }
        }

        // GET: api/Identifiers/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetIdentifier(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lấy dữ liệu thất bại",
                        error = "ID không hợp lệ"
                    });
                }

                var identifier = await _context.Identifiers
                    .Include(i => i.Document)
                    .Where(i => i.ID == id)
                    .Select(i => new
                    {
                        i.ID,
                        i.DocumentID,
                        i.Type,
                        i.Value,
                        Document = new
                        {
                            i.Document.ID,
                            i.Document.Title
                        }
                    })
                    .FirstOrDefaultAsync();

                if (identifier == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy định danh",
                        error = $"Định danh với ID {id} không tồn tại"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin định danh thành công",
                    data = identifier
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy thông tin định danh",
                    error = ex.Message
                });
            }
        }

        // GET: api/Identifiers/document/{documentId}
        [HttpGet("document/{documentId}")]
        public async Task<ActionResult<object>> GetIdentifiersByDocument(string documentId)
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

                var documentExists = await _context.Documents.AnyAsync(d => d.ID == documentId);
                if (!documentExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy tài liệu",
                        error = $"Tài liệu với ID {documentId} không tồn tại"
                    });
                }

                var identifiers = await _context.Identifiers
                    .Where(i => i.DocumentID == documentId)
                    .Select(i => new
                    {
                        i.ID,
                        i.DocumentID,
                        i.Type,
                        i.Value
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách định danh của tài liệu thành công",
                    data = identifiers,
                    count = identifiers.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách định danh của tài liệu",
                    error = ex.Message
                });
            }
        }

        // GET: api/Identifiers/type/{type}
        [HttpGet("type/{type}")]
        public async Task<ActionResult<object>> GetIdentifiersByType(string type)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(type))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lấy dữ liệu thất bại",
                        error = "Type không được để trống"
                    });
                }

                var identifiers = await _context.Identifiers
                    .Where(i => i.Type == type)
                    .Include(i => i.Document)
                    .Select(i => new
                    {
                        i.ID,
                        i.DocumentID,
                        i.Type,
                        i.Value,
                        Document = new
                        {
                            i.Document.ID,
                            i.Document.Title
                        }
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Lấy danh sách định danh loại '{type}' thành công",
                    data = identifiers,
                    count = identifiers.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách định danh theo loại",
                    error = ex.Message
                });
            }
        }

        // GET: api/Identifiers/search/{value}
        [HttpGet("search/{value}")]
        public async Task<ActionResult<object>> SearchIdentifierByValue(string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tìm kiếm thất bại",
                        error = "Giá trị tìm kiếm không được để trống"
                    });
                }

                var identifier = await _context.Identifiers
                    .Where(i => i.Value == value)
                    .Include(i => i.Document)
                    .Select(i => new
                    {
                        i.ID,
                        i.DocumentID,
                        i.Type,
                        i.Value,
                        Document = new
                        {
                            i.Document.ID,
                            i.Document.Title
                        }
                    })
                    .FirstOrDefaultAsync();

                if (identifier == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy định danh",
                        error = $"Không tìm thấy định danh với giá trị '{value}'"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Tìm kiếm định danh thành công",
                    data = identifier
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi tìm kiếm định danh",
                    error = ex.Message
                });
            }
        }

        // POST: api/Identifiers
        [HttpPost]
        public async Task<ActionResult<object>> PostIdentifier(IdentifierCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.DocumentID))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo định danh thất bại",
                        error = "Document ID không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.Type))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo định danh thất bại",
                        error = "Type không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.Value))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo định danh thất bại",
                        error = "Value không được để trống"
                    });
                }

                // Kiểm tra Document tồn tại
                var documentExists = await _context.Documents.AnyAsync(d => d.ID == dto.DocumentID);
                if (!documentExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Tạo định danh thất bại",
                        error = $"Tài liệu với ID {dto.DocumentID} không tồn tại"
                    });
                }

                // Kiểm tra trùng lặp (cùng DocumentID, Type và Value)
                var exists = await _context.Identifiers
                    .AnyAsync(i => i.DocumentID == dto.DocumentID &&
                                   i.Type == dto.Type &&
                                   i.Value == dto.Value);
                if (exists)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Tạo định danh thất bại",
                        error = "Định danh với Type và Value này đã tồn tại cho tài liệu này"
                    });
                }

                var identifier = new Identifier
                {
                    ID = Guid.NewGuid(),
                    DocumentID = dto.DocumentID,
                    Type = dto.Type.Trim(),
                    Value = dto.Value.Trim()
                };

                _context.Identifiers.Add(identifier);
                await _context.SaveChangesAsync();

                var result = await _context.Identifiers
                    .Include(i => i.Document)
                    .Where(i => i.ID == identifier.ID)
                    .Select(i => new
                    {
                        i.ID,
                        i.DocumentID,
                        i.Type,
                        i.Value,
                        Document = new
                        {
                            i.Document.ID,
                            i.Document.Title
                        }
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetIdentifier),
                    new { id = identifier.ID },
                    new
                    {
                        success = true,
                        message = "Tạo định danh thành công",
                        data = result
                    });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lưu định danh vào database",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi tạo định danh",
                    error = ex.Message
                });
            }
        }

        // PUT: api/Identifiers/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> PutIdentifier(Guid id, IdentifierUpdateDto dto)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "ID không hợp lệ"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.Type))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Type không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.Value))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Value không được để trống"
                    });
                }

                var identifier = await _context.Identifiers.FindAsync(id);
                if (identifier == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = $"Định danh với ID {id} không tồn tại"
                    });
                }

                // Kiểm tra trùng lặp (ngoại trừ chính nó)
                var exists = await _context.Identifiers
                    .AnyAsync(i => i.ID != id &&
                                   i.DocumentID == identifier.DocumentID &&
                                   i.Type == dto.Type &&
                                   i.Value == dto.Value);
                if (exists)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Định danh với Type và Value này đã tồn tại cho tài liệu này"
                    });
                }

                identifier.Type = dto.Type.Trim();
                identifier.Value = dto.Value.Trim();

                _context.Entry(identifier).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var result = await _context.Identifiers
                    .Include(i => i.Document)
                    .Where(i => i.ID == id)
                    .Select(i => new
                    {
                        i.ID,
                        i.DocumentID,
                        i.Type,
                        i.Value,
                        Document = new
                        {
                            i.Document.ID,
                            i.Document.Title
                        }
                    })
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật định danh thành công",
                    data = result
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Identifiers.AnyAsync(i => i.ID == id))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Định danh không tồn tại hoặc đã bị xóa"
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Xung đột dữ liệu. Định danh có thể đã được cập nhật bởi người dùng khác"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi cập nhật định danh",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/Identifiers/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteIdentifier(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Xóa thất bại",
                        error = "ID không hợp lệ"
                    });
                }

                var identifier = await _context.Identifiers.FindAsync(id);

                if (identifier == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Xóa thất bại",
                        error = $"Định danh với ID {id} không tồn tại"
                    });
                }

                _context.Identifiers.Remove(identifier);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Xóa định danh thành công",
                    deletedId = id
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Xóa thất bại",
                    error = "Không thể xóa định danh. Có thể đang được sử dụng bởi dữ liệu khác",
                    details = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi xóa định danh",
                    error = ex.Message
                });
            }
        }
    }
}