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
    public class ResearchPublicationsController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public ResearchPublicationsController(DigitalLibraryContext context)
        {
            _context = context;
        }

        // GET: api/ResearchPublications
        [HttpGet]
        public async Task<ActionResult<object>> GetResearchPublications()
        {
            try
            {
                var publications = await _context.ResearchPublications
                    .Include(rp => rp.Document)
                    .Select(rp => new
                    {
                        rp.DocumentID,
                        rp.VenueName,
                        rp.PublicationType,
                        Document = new
                        {
                            rp.Document.DocumentId,
                            rp.Document.Title,
                            rp.Document.CreatedAt
                        }
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách bài báo khoa học thành công",
                    data = publications,
                    count = publications.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách bài báo khoa học",
                    error = ex.Message
                });
            }
        }

        // GET: api/ResearchPublications/{documentId}
        [HttpGet("{documentId}")]
        public async Task<ActionResult<object>> GetResearchPublication(string documentId)
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

                var publication = await _context.ResearchPublications
                    .Include(rp => rp.Document)
                    .Where(rp => rp.DocumentID == documentId)
                    .Select(rp => new
                    {
                        rp.DocumentID,
                        rp.VenueName,
                        rp.PublicationType,
                        //Document = new
                        //{
                        //    rp.Document.ID,
                        //    rp.Document.Title,
                        //    rp.Document.CreatedAt,
                        //    rp.Document.UpdatedAt
                        //}
                    })
                    .FirstOrDefaultAsync();

                if (publication == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy bài báo khoa học",
                        error = $"Bài báo khoa học với Document ID {documentId} không tồn tại"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin bài báo khoa học thành công",
                    data = publication
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy thông tin bài báo khoa học",
                    error = ex.Message
                });
            }
        }

        // GET: api/ResearchPublications/venue/{venueName}
        [HttpGet("venue/{venueName}")]
        public async Task<ActionResult<object>> GetPublicationsByVenue(string venueName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(venueName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lấy dữ liệu thất bại",
                        error = "Venue Name không được để trống"
                    });
                }

                var publications = await _context.ResearchPublications
                    .Where(rp => rp.VenueName.Contains(venueName))
                    .Include(rp => rp.Document)
                    .Select(rp => new
                    {
                        rp.DocumentID,
                        rp.VenueName,
                        rp.PublicationType,
                        Document = new
                        {
                            rp.Document.DocumentId,
                            rp.Document.Title
                        }
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Lấy danh sách bài báo từ '{venueName}' thành công",
                    data = publications,
                    count = publications.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách bài báo theo venue",
                    error = ex.Message
                });
            }
        }

        // GET: api/ResearchPublications/type/{publicationType}
        [HttpGet("type/{publicationType}")]
        public async Task<ActionResult<object>> GetPublicationsByType(string publicationType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(publicationType))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lấy dữ liệu thất bại",
                        error = "Publication Type không được để trống"
                    });
                }

                var publications = await _context.ResearchPublications
                    .Where(rp => rp.PublicationType == publicationType)
                    .Include(rp => rp.Document)
                    .Select(rp => new
                    {
                        rp.DocumentID,
                        rp.VenueName,
                        rp.PublicationType,
                        Document = new
                        {
                            rp.Document.DocumentId,
                            rp.Document.Title
                        }
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Lấy danh sách bài báo loại '{publicationType}' thành công",
                    data = publications,
                    count = publications.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách bài báo theo loại",
                    error = ex.Message
                });
            }
        }

        // POST: api/ResearchPublications
        [HttpPost]
        public async Task<ActionResult<object>> PostResearchPublication(ResearchPublicationCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.DocumentID))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo bài báo khoa học thất bại",
                        error = "Document ID không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.VenueName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo bài báo khoa học thất bại",
                        error = "Venue Name không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.PublicationType))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo bài báo khoa học thất bại",
                        error = "Publication Type không được để trống"
                    });
                }

                // Kiểm tra Document tồn tại
                var documentExists = await _context.Documents.AnyAsync(d => d.DocumentId == dto.DocumentID);
                if (!documentExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Tạo bài báo khoa học thất bại",
                        error = $"Tài liệu với ID {dto.DocumentID} không tồn tại"
                    });
                }

                // Kiểm tra bài báo đã tồn tại chưa
                var exists = await _context.ResearchPublications.AnyAsync(rp => rp.DocumentID == dto.DocumentID);
                if (exists)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Tạo bài báo khoa học thất bại",
                        error = $"Bài báo khoa học với Document ID {dto.DocumentID} đã tồn tại"
                    });
                }

                var publication = new ResearchPublication
                {
                    DocumentID = dto.DocumentID,
                    VenueName = dto.VenueName.Trim(),
                    PublicationType = dto.PublicationType.Trim()
                };

                _context.ResearchPublications.Add(publication);
                await _context.SaveChangesAsync();

                var result = await _context.ResearchPublications
                    .Include(rp => rp.Document)
                    .Where(rp => rp.DocumentID == publication.DocumentID)
                    .Select(rp => new
                    {
                        rp.DocumentID,
                        rp.VenueName,
                        rp.PublicationType,
                        Document = new
                        {
                            rp.Document.DocumentId,
                            rp.Document.Title
                        }
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetResearchPublication),
                    new { documentId = publication.DocumentID },
                    new
                    {
                        success = true,
                        message = "Tạo bài báo khoa học thành công",
                        data = result
                    });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lưu bài báo khoa học vào database",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi tạo bài báo khoa học",
                    error = ex.Message
                });
            }
        }

        // PUT: api/ResearchPublications/{documentId}
        [HttpPut("{documentId}")]
        public async Task<ActionResult<object>> PutResearchPublication(string documentId, ResearchPublicationUpdateDto dto)
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

                if (string.IsNullOrWhiteSpace(dto.VenueName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Venue Name không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.PublicationType))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Publication Type không được để trống"
                    });
                }

                var publication = await _context.ResearchPublications.FindAsync(documentId);
                if (publication == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = $"Bài báo khoa học với Document ID {documentId} không tồn tại"
                    });
                }

                publication.VenueName = dto.VenueName.Trim();
                publication.PublicationType = dto.PublicationType.Trim();

                _context.Entry(publication).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var result = await _context.ResearchPublications
                    .Include(rp => rp.Document)
                    .Where(rp => rp.DocumentID == documentId)
                    .Select(rp => new
                    {
                        rp.DocumentID,
                        rp.VenueName,
                        rp.PublicationType,
                        Document = new
                        {
                            rp.Document.DocumentId,
                            rp.Document.Title
                        }
                    })
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật bài báo khoa học thành công",
                    data = result
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.ResearchPublications.AnyAsync(rp => rp.DocumentID == documentId))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Bài báo khoa học không tồn tại hoặc đã bị xóa"
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Xung đột dữ liệu. Bài báo khoa học có thể đã được cập nhật bởi người dùng khác"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi cập nhật bài báo khoa học",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/ResearchPublications/{documentId}
        [HttpDelete("{documentId}")]
        public async Task<ActionResult<object>> DeleteResearchPublication(string documentId)
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

                var publication = await _context.ResearchPublications.FindAsync(documentId);

                if (publication == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Xóa thất bại",
                        error = $"Bài báo khoa học với Document ID {documentId} không tồn tại"
                    });
                }

                _context.ResearchPublications.Remove(publication);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Xóa bài báo khoa học thành công",
                    deletedDocumentId = documentId
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Xóa thất bại",
                    error = "Không thể xóa bài báo khoa học. Có thể đang được sử dụng bởi dữ liệu khác",
                    details = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi xóa bài báo khoa học",
                    error = ex.Message
                });
            }
        }
    }
}