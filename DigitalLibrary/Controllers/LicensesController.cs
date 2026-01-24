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
    public class LicensesController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public LicensesController(DigitalLibraryContext context)
        {
            _context = context;
        }

        // GET: api/Licenses
        [HttpGet]
        public async Task<ActionResult<object>> GetLicenses()
        {
            try
            {
                var licenses = await _context.Licenses.ToListAsync();
                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách giấy phép thành công",
                    data = licenses,
                    count = licenses.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách giấy phép",
                    error = ex.Message
                });
            }
        }

        // GET: api/Licenses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetLicense(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ID không hợp lệ",
                        error = "ID không được để trống"
                    });
                }

                var license = await _context.Licenses.FindAsync(id);

                if (license == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy giấy phép",
                        error = $"Giấy phép với ID {id} không tồn tại"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin giấy phép thành công",
                    data = license
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy thông tin giấy phép",
                    error = ex.Message
                });
            }
        }

        // PUT: api/Licenses/5
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> PutLicense(Guid id, License license)
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

                if (id != license.ID)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "ID trong URL và ID trong body không khớp"
                    });
                }

                if (string.IsNullOrWhiteSpace(license.Name))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Tên giấy phép không được để trống"
                    });
                }

                var existingLicense = await _context.Licenses.FindAsync(id);
                if (existingLicense == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = $"Giấy phép với ID {id} không tồn tại"
                    });
                }

                _context.Entry(license).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật giấy phép thành công",
                    data = license
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LicenseExists(id))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Giấy phép không tồn tại hoặc đã bị xóa"
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Xung đột dữ liệu. Giấy phép có thể đã được cập nhật bởi người dùng khác"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi cập nhật giấy phép",
                    error = ex.Message
                });
            }
        }

        // POST: api/Licenses
        [HttpPost]
        public async Task<ActionResult<object>> PostLicense(LicenseCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo giấy phép thất bại",
                        error = "Tên giấy phép không được để trống"
                    });
                }

                // Kiểm tra trùng tên
                var existingLicense = await _context.Licenses
                    .FirstOrDefaultAsync(l => l.Name.ToLower() == dto.Name.ToLower());

                if (existingLicense != null)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Tạo giấy phép thất bại",
                        error = $"Giấy phép với tên '{dto.Name}' đã tồn tại"
                    });
                }

                var license = new License
                {
                    ID = Guid.NewGuid(),
                    Name = dto.Name,
                    Content = dto.Content
                };

                _context.Licenses.Add(license);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetLicense),
                    new { id = license.ID },
                    new
                    {
                        success = true,
                        message = "Tạo giấy phép thành công",
                        data = license
                    });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lưu giấy phép vào database",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi tạo giấy phép",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/Licenses/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteLicense(Guid id)
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

                var license = await _context.Licenses.FindAsync(id);

                if (license == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Xóa thất bại",
                        error = $"Giấy phép với ID {id} không tồn tại"
                    });
                }

                _context.Licenses.Remove(license);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Xóa giấy phép thành công",
                    deletedId = id
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Xóa thất bại",
                    error = "Không thể xóa giấy phép. Có thể đang được sử dụng bởi dữ liệu khác",
                    details = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi xóa giấy phép",
                    error = ex.Message
                });
            }
        }

        private bool LicenseExists(Guid id)
        {
            return _context.Licenses.Any(e => e.ID == id);
        }
    }
}