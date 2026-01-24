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
    public class KeywordsController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public KeywordsController(DigitalLibraryContext context)
        {
            _context = context;
        }

        // GET: api/Keywords
        /// <summary>
        /// Lấy danh sách tất cả từ khóa
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<object>> GetKeywords()
        {
            try
            {
                var keywords = await _context.Keywords.ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách từ khóa thành công",
                    data = keywords,
                    count = keywords.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách từ khóa",
                    error = ex.Message
                });
            }
        }

        // GET: api/Keywords/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetKeyword(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lấy dữ liệu thất bại",
                        error = "ID không được để trống"
                    });
                }

                var keyword = await _context.Keywords.FindAsync(id);

                if (keyword == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy từ khóa",
                        error = $"Từ khóa với ID '{id}' không tồn tại"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin từ khóa thành công",
                    data = keyword
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy thông tin từ khóa",
                    error = ex.Message
                });
            }
        }

        // POST: api/Keywords
        [HttpPost]
        public async Task<ActionResult<object>> PostKeyword(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo từ khóa thất bại",
                        error = "Tên từ khóa không được để trống"
                    });
                }

                // Tạo ID từ Name (loại bỏ khoảng trắng, chuyển thành lowercase)
                var keywordName = name.Trim().ToLower().Replace(" ", "-");

                // Kiểm tra ID đã tồn tại chưa
                var existsById = await _context.Keywords.AnyAsync(k => k.Name == keywordName);
                if (existsById)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Tạo từ khóa thất bại",
                        error = $"Từ khóa '{keywordName}' đã tồn tại"
                    });
                }

                var keyword = new Keyword
                {
                    ID = keywordName,
                    Name = name
                };

                _context.Keywords.Add(keyword);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetKeyword),
                    new { id = keyword.ID },
                    new
                    {
                        success = true,
                        message = "Tạo từ khóa thành công",
                        data = keyword
                    });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lưu từ khóa vào database",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi tạo từ khóa",
                    error = ex.Message
                });
            }
        }

        // PUT: api/Keywords/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> PutKeyword(string id, string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "ID không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Tên từ khóa không được để trống"
                    });
                }

                var keyword = await _context.Keywords.FindAsync(id);
                if (keyword == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = $"Từ khóa với ID '{id}' không tồn tại"
                    });
                }

                keyword.Name = name;
                _context.Entry(keyword).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật từ khóa thành công",
                    data = keyword
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Keywords.AnyAsync(k => k.ID == id))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Từ khóa không tồn tại hoặc đã bị xóa"
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Xung đột dữ liệu. Từ khóa có thể đã được cập nhật bởi người dùng khác"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi cập nhật từ khóa",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/Keywords/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteKeyword(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Xóa thất bại",
                        error = "ID không được để trống"
                    });
                }

                var keyword = await _context.Keywords.FindAsync(id);

                if (keyword == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Xóa thất bại",
                        error = $"Từ khóa với ID '{id}' không tồn tại"
                    });
                }

                _context.Keywords.Remove(keyword);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Xóa từ khóa thành công",
                    deletedId = id
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Xóa thất bại",
                    error = "Không thể xóa từ khóa. Có thể đang được sử dụng bởi dữ liệu khác",
                    details = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi xóa từ khóa",
                    error = ex.Message
                });
            }
        }
    }
}