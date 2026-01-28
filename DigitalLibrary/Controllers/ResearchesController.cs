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
    public class ResearchesController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public ResearchesController(DigitalLibraryContext context)
        {
            _context = context;
        }

        // GET: api/Researches
        [HttpGet]
        public async Task<ActionResult<object>> GetResearches()
        {
            try
            {
                var researches = await _context.Researches
                    .Include(r => r.Document)
                    .Select(r => new
                    {
                        r.DocumentID,
                        r.Abstract,
                        r.ResearchLevel,
                        Document = new
                        {
                            r.Document.ID,
                            r.Document.Title,
                            r.Document.CreatedAt
                        }
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách nghiên cứu thành công",
                    data = researches,
                    count = researches.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách nghiên cứu",
                    error = ex.Message
                });
            }
        }

        // GET: api/Researches/{documentId}
        [HttpGet("{documentId}")]
        public async Task<ActionResult<object>> GetResearch(string documentId)
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

                var research = await _context.Researches
                    .Include(r => r.Document)
                    .Where(r => r.DocumentID == documentId)
                    .Select(r => new
                    {
                        r.DocumentID,
                        r.Abstract,
                        r.ResearchLevel,
                        //Document = new
                        //{
                        //    r.Document.ID,
                        //    r.Document.Title,
                        //    r.Document.CreatedAt,
                        //    r.Document.UpdatedAt
                        //}
                    })
                    .FirstOrDefaultAsync();

                if (research == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy nghiên cứu",
                        error = $"Nghiên cứu với Document ID {documentId} không tồn tại"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin nghiên cứu thành công",
                    data = research
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy thông tin nghiên cứu",
                    error = ex.Message
                });
            }
        }

        // GET: api/Researches/level/{researchLevel}
        [HttpGet("level/{researchLevel}")]
        public async Task<ActionResult<object>> GetResearchesByLevel(string researchLevel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(researchLevel))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lấy dữ liệu thất bại",
                        error = "Research Level không được để trống"
                    });
                }

                var researches = await _context.Researches
                    .Where(r => r.ResearchLevel == researchLevel)
                    .Include(r => r.Document)
                    .Select(r => new
                    {
                        r.DocumentID,
                        r.Abstract,
                        r.ResearchLevel,
                        Document = new
                        {
                            r.Document.ID,
                            r.Document.Title
                        }
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Lấy danh sách nghiên cứu cấp '{researchLevel}' thành công",
                    data = researches,
                    count = researches.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách nghiên cứu theo cấp",
                    error = ex.Message
                });
            }
        }

        // POST: api/Researches
        [HttpPost]
        public async Task<ActionResult<object>> PostResearch(ResearchCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.DocumentID))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo nghiên cứu thất bại",
                        error = "Document ID không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.ResearchLevel))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo nghiên cứu thất bại",
                        error = "Research Level không được để trống"
                    });
                }

                // Kiểm tra Document tồn tại
                var documentExists = await _context.Documents.AnyAsync(d => d.ID == dto.DocumentID);
                if (!documentExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Tạo nghiên cứu thất bại",
                        error = $"Tài liệu với ID {dto.DocumentID} không tồn tại"
                    });
                }

                // Kiểm tra nghiên cứu đã tồn tại chưa
                var exists = await _context.Researches.AnyAsync(r => r.DocumentID == dto.DocumentID);
                if (exists)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Tạo nghiên cứu thất bại",
                        error = $"Nghiên cứu với Document ID {dto.DocumentID} đã tồn tại"
                    });
                }

                var research = new Research
                {
                    DocumentID = dto.DocumentID,
                    Abstract = dto.Abstract?.Trim(),
                    ResearchLevel = dto.ResearchLevel.Trim()
                };

                _context.Researches.Add(research);
                await _context.SaveChangesAsync();

                var result = await _context.Researches
                    .Include(r => r.Document)
                    .Where(r => r.DocumentID == research.DocumentID)
                    .Select(r => new
                    {
                        r.DocumentID,
                        r.Abstract,
                        r.ResearchLevel,
                        Document = new
                        {
                            r.Document.ID,
                            r.Document.Title
                        }
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetResearch),
                    new { documentId = research.DocumentID },
                    new
                    {
                        success = true,
                        message = "Tạo nghiên cứu thành công",
                        data = result
                    });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lưu nghiên cứu vào database",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi tạo nghiên cứu",
                    error = ex.Message
                });
            }
        }

        // PUT: api/Researches/{documentId}
        [HttpPut("{documentId}")]
        public async Task<ActionResult<object>> PutResearch(string documentId, ResearchUpdateDto dto)
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

                if (string.IsNullOrWhiteSpace(dto.ResearchLevel))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Research Level không được để trống"
                    });
                }

                var research = await _context.Researches.FindAsync(documentId);
                if (research == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = $"Nghiên cứu với Document ID {documentId} không tồn tại"
                    });
                }

                research.Abstract = dto.Abstract?.Trim();
                research.ResearchLevel = dto.ResearchLevel.Trim();

                _context.Entry(research).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var result = await _context.Researches
                    .Include(r => r.Document)
                    .Where(r => r.DocumentID == documentId)
                    .Select(r => new
                    {
                        r.DocumentID,
                        r.Abstract,
                        r.ResearchLevel,
                        Document = new
                        {
                            r.Document.ID,
                            r.Document.Title
                        }
                    })
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật nghiên cứu thành công",
                    data = result
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Researches.AnyAsync(r => r.DocumentID == documentId))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Nghiên cứu không tồn tại hoặc đã bị xóa"
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Xung đột dữ liệu. Nghiên cứu có thể đã được cập nhật bởi người dùng khác"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi cập nhật nghiên cứu",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/Researches/{documentId}
        [HttpDelete("{documentId}")]
        public async Task<ActionResult<object>> DeleteResearch(string documentId)
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

                var research = await _context.Researches.FindAsync(documentId);

                if (research == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Xóa thất bại",
                        error = $"Nghiên cứu với Document ID {documentId} không tồn tại"
                    });
                }

                _context.Researches.Remove(research);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Xóa nghiên cứu thành công",
                    deletedDocumentId = documentId
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Xóa thất bại",
                    error = "Không thể xóa nghiên cứu. Có thể đang được sử dụng bởi dữ liệu khác",
                    details = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi xóa nghiên cứu",
                    error = ex.Message
                });
            }
        }
    }
}