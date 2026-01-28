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
    public class DocumentLicensesController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public DocumentLicensesController(DigitalLibraryContext context)
        {
            _context = context;
        }

        // GET: api/DocumentLicenses
        [HttpGet]
        public async Task<ActionResult<object>> GetDocumentLicenses()
        {
            try
            {
                var documentLicenses = await _context.Document_Licenses
                    .Include(dl => dl.Document)
                    .Include(dl => dl.License)
                    .Select(dl => new
                    {
                        dl.DocumentID,
                        dl.LicenseID,
                        dl.AcceptedAt,
                        //Document = new
                        //{
                        //    dl.Document.ID,
                        //    dl.Document.Title
                        //    // Thêm các field khác của Document nếu cần
                        //},
                        //License = new
                        //{
                        //    dl.License.ID,
                        //    dl.License.Name,
                        //    dl.License.Content
                        //}
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách giấy phép tài liệu thành công",
                    data = documentLicenses,
                    count = documentLicenses.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách giấy phép tài liệu",
                    error = ex.Message
                });
            }
        }

        // GET: api/DocumentLicenses/document/{documentId}
        [HttpGet("document/{documentId}")]
        public async Task<ActionResult<object>> GetLicensesByDocument(string documentId)
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

                var documentExists = await _context.Documents.AnyAsync(d => d.DocumentId == documentId);
                if (!documentExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy tài liệu",
                        error = $"Tài liệu với ID {documentId} không tồn tại"
                    });
                }

                var licenses = await _context.Document_Licenses
                    .Where(dl => dl.DocumentID == documentId)
                    .Include(dl => dl.License)
                    .Select(dl => new
                    {
                        dl.DocumentID,
                        dl.LicenseID,
                        dl.AcceptedAt,
                        //License = new
                        //{
                        //    dl.License.ID,
                        //    dl.License.Name,
                        //    dl.License.Content
                        //}
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách giấy phép của tài liệu thành công",
                    data = licenses,
                    count = licenses.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách giấy phép của tài liệu",
                    error = ex.Message
                });
            }
        }

        // GET: api/DocumentLicenses/license/{licenseId}
        [HttpGet("license/{licenseId}")]
        public async Task<ActionResult<object>> GetDocumentsByLicense(Guid licenseId)
        {
            try
            {
                if (licenseId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lấy dữ liệu thất bại",
                        error = "License ID không hợp lệ"
                    });
                }

                var licenseExists = await _context.Licenses.AnyAsync(l => l.ID == licenseId);
                if (!licenseExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy giấy phép",
                        error = $"Giấy phép với ID {licenseId} không tồn tại"
                    });
                }

                var documents = await _context.Document_Licenses
                    .Where(dl => dl.LicenseID == licenseId)
                    .Include(dl => dl.Document)
                    .Select(dl => new
                    {
                        dl.DocumentID,
                        dl.LicenseID,
                        dl.AcceptedAt,
                        //Document = new
                        //{
                        //    dl.Document.ID,
                        //    dl.Document.Title
                        //    // Thêm các field khác của Document nếu cần
                        //}
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách tài liệu theo giấy phép thành công",
                    data = documents,
                    count = documents.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách tài liệu theo giấy phép",
                    error = ex.Message
                });
            }
        }

        // GET: api/DocumentLicenses/{documentId}/{licenseId}
        [HttpGet("{documentId}/{licenseId}")]
        public async Task<ActionResult<object>> GetDocumentLicense(string documentId, Guid licenseId)
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

                if (licenseId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lấy dữ liệu thất bại",
                        error = "License ID không hợp lệ"
                    });
                }

                var documentLicense = await _context.Document_Licenses
                    .Include(dl => dl.Document)
                    .Include(dl => dl.License)
                    .Where(dl => dl.DocumentID == documentId && dl.LicenseID == licenseId)
                    .Select(dl => new
                    {
                        dl.DocumentID,
                        dl.LicenseID,
                        dl.AcceptedAt,
                        //Document = new
                        //{
                        //    dl.Document.ID,
                        //    dl.Document.Title
                        //},
                        //License = new
                        //{
                        //    dl.License.ID,
                        //    dl.License.Name,
                        //    dl.License.Content
                        //}
                    })
                    .FirstOrDefaultAsync();

                if (documentLicense == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy quan hệ giấy phép - tài liệu",
                        error = $"Không tìm thấy quan hệ giữa tài liệu {documentId} và giấy phép {licenseId}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin giấy phép tài liệu thành công",
                    data = documentLicense
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy thông tin giấy phép tài liệu",
                    error = ex.Message
                });
            }
        }

        // POST: api/DocumentLicenses
        [HttpPost]
        public async Task<ActionResult<object>> PostDocumentLicense(DocumentLicenseCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.DocumentID))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Thêm giấy phép thất bại",
                        error = "Document ID không được để trống"
                    });
                }

                if (dto.LicenseID == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Thêm giấy phép thất bại",
                        error = "License ID không hợp lệ"
                    });
                }

                // Kiểm tra Document tồn tại
                var documentExists = await _context.Documents.AnyAsync(d => d.DocumentId == dto.DocumentID);
                if (!documentExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Thêm giấy phép thất bại",
                        error = $"Tài liệu với ID {dto.DocumentID} không tồn tại"
                    });
                }

                // Kiểm tra License tồn tại
                var licenseExists = await _context.Licenses.AnyAsync(l => l.ID == dto.LicenseID);
                if (!licenseExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Thêm giấy phép thất bại",
                        error = $"Giấy phép với ID {dto.LicenseID} không tồn tại"
                    });
                }

                // Kiểm tra quan hệ đã tồn tại chưa
                var exists = await _context.Document_Licenses
                    .AnyAsync(dl => dl.DocumentID == dto.DocumentID && dl.LicenseID == dto.LicenseID);

                if (exists)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Thêm giấy phép thất bại",
                        error = "Tài liệu này đã được gán giấy phép này rồi"
                    });
                }

                var documentLicense = new Document_License
                {
                    DocumentID = dto.DocumentID,
                    LicenseID = dto.LicenseID,
                    AcceptedAt = DateTime.UtcNow
                };

                _context.Document_Licenses.Add(documentLicense);
                await _context.SaveChangesAsync();

                // Lấy thông tin đầy đủ để trả về
                var result = await _context.Document_Licenses
                    .Include(dl => dl.Document)
                    .Include(dl => dl.License)
                    .Where(dl => dl.DocumentID == documentLicense.DocumentID &&
                                 dl.LicenseID == documentLicense.LicenseID)
                    .Select(dl => new
                    {
                        dl.DocumentID,
                        dl.LicenseID,
                        dl.AcceptedAt,
                        //Document = new
                        //{
                        //    dl.Document.ID,
                        //    dl.Document.Title
                        //},
                        //License = new
                        //{
                        //    dl.License.ID,
                        //    dl.License.Name,
                        //    dl.License.Content
                        //}
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetDocumentLicense),
                    new { documentId = documentLicense.DocumentID, licenseId = documentLicense.LicenseID },
                    new
                    {
                        success = true,
                        message = "Thêm giấy phép cho tài liệu thành công",
                        data = result
                    });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lưu giấy phép tài liệu vào database",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi thêm giấy phép cho tài liệu",
                    error = ex.Message
                });
            }
        }

        // PUT: api/DocumentLicenses/{documentId}/{licenseId}
        [HttpPut("{documentId}/{licenseId}")]
        public async Task<ActionResult<object>> UpdateDocumentLicense(
            string documentId,
            Guid licenseId,
            DateTime acpAt)
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

                if (licenseId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "License ID không hợp lệ"
                    });
                }

                var documentLicense = await _context.Document_Licenses
                    .FirstOrDefaultAsync(dl => dl.DocumentID == documentId && dl.LicenseID == licenseId);

                if (documentLicense == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Không tìm thấy quan hệ giấy phép - tài liệu"
                    });
                }

                // Cập nhật AcceptedAt nếu có trong DTO
               
                documentLicense.AcceptedAt = acpAt;
                

                _context.Entry(documentLicense).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var result = await _context.Document_Licenses
                    .Include(dl => dl.Document)
                    .Include(dl => dl.License)
                    .Where(dl => dl.DocumentID == documentId && dl.LicenseID == licenseId)
                    .Select(dl => new
                    {
                        dl.DocumentID,
                        dl.LicenseID,
                        dl.AcceptedAt,
                        //Document = new
                        //{
                        //    dl.Document.ID,
                        //    dl.Document.Title
                        //},
                        //License = new
                        //{
                        //    dl.License.ID,
                        //    dl.License.Name
                        //}
                    })
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật giấy phép tài liệu thành công",
                    data = result
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Cập nhật thất bại",
                    error = "Xung đột dữ liệu. Dữ liệu có thể đã được cập nhật bởi người dùng khác"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi cập nhật giấy phép tài liệu",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/DocumentLicenses/{documentId}/{licenseId}
        [HttpDelete("{documentId}/{licenseId}")]
        public async Task<ActionResult<object>> DeleteDocumentLicense(string documentId, Guid licenseId)
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

                if (licenseId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Xóa thất bại",
                        error = "License ID không hợp lệ"
                    });
                }

                var documentLicense = await _context.Document_Licenses
                    .FirstOrDefaultAsync(dl => dl.DocumentID == documentId && dl.LicenseID == licenseId);

                if (documentLicense == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Xóa thất bại",
                        error = "Không tìm thấy quan hệ giấy phép - tài liệu"
                    });
                }

                _context.Document_Licenses.Remove(documentLicense);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Xóa giấy phép khỏi tài liệu thành công",
                    deletedDocumentId = documentId,
                    deletedLicenseId = licenseId
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Xóa thất bại",
                    error = "Không thể xóa giấy phép tài liệu",
                    details = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi xóa giấy phép tài liệu",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/DocumentLicenses/document/{documentId}
        [HttpDelete("document/{documentId}")]
        public async Task<ActionResult<object>> DeleteAllLicensesFromDocument(string documentId)
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

                var documentLicenses = await _context.Document_Licenses
                    .Where(dl => dl.DocumentID == documentId)
                    .ToListAsync();

                if (!documentLicenses.Any())
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy giấy phép nào",
                        error = $"Tài liệu {documentId} không có giấy phép nào"
                    });
                }

                _context.Document_Licenses.RemoveRange(documentLicenses);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Xóa tất cả giấy phép khỏi tài liệu thành công",
                    deletedCount = documentLicenses.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi xóa giấy phép khỏi tài liệu",
                    error = ex.Message
                });
            }
        }
    }
}