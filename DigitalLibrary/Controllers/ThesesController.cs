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
    public class ThesesController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public ThesesController(DigitalLibraryContext context)
        {
            _context = context;
        }

        // GET: api/Theses
        [HttpGet]
        public async Task<ActionResult<object>> GetTheses()
        {
            try
            {
                var theses = await _context.Theses
                    .Include(t => t.Document)
                    .Select(t => new
                    {
                        t.DocumentID,
                        t.DegreeLevel,
                        t.Discipline,
                        t.AdvisorName,
                        t.Abstract,
                        //Document = new
                        //{
                        //    t.Document.ID,
                        //    t.Document.Title,
                        //    t.Document.CreatedAt
                        //}
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách luận văn thành công",
                    data = theses,
                    count = theses.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách luận văn",
                    error = ex.Message
                });
            }
        }

        // GET: api/Theses/{documentId}
        [HttpGet("{documentId}")]
        public async Task<ActionResult<object>> GetThesis(string documentId)
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

                var thesis = await _context.Theses
                    .Include(t => t.Document)
                    .Where(t => t.DocumentID == documentId)
                    .Select(t => new
                    {
                        t.DocumentID,
                        t.DegreeLevel,
                        t.Discipline,
                        t.AdvisorName,
                        t.Abstract,
                        //Document = new
                        //{
                        //    t.Document.ID,
                        //    t.Document.Title,
                        //    t.Document.CreatedAt,
                        //    t.Document.UpdatedAt
                        //}
                    })
                    .FirstOrDefaultAsync();

                if (thesis == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy luận văn",
                        error = $"Luận văn với Document ID {documentId} không tồn tại"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin luận văn thành công",
                    data = thesis
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy thông tin luận văn",
                    error = ex.Message
                });
            }
        }

        // GET: api/Theses/degree/{degreeLevel}
        [HttpGet("degree/{degreeLevel}")]
        public async Task<ActionResult<object>> GetThesesByDegree(string degreeLevel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(degreeLevel))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lấy dữ liệu thất bại",
                        error = "Degree Level không được để trống"
                    });
                }

                var theses = await _context.Theses
                    .Where(t => t.DegreeLevel == degreeLevel)
                    .Include(t => t.Document)
                    .Select(t => new
                    {
                        t.DocumentID,
                        t.DegreeLevel,
                        t.Discipline,
                        t.AdvisorName,
                        //Document = new
                        //{
                        //    t.Document.ID,
                        //    t.Document.Title
                        //}
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Lấy danh sách luận văn bậc '{degreeLevel}' thành công",
                    data = theses,
                    count = theses.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách luận văn theo bậc học",
                    error = ex.Message
                });
            }
        }

        // GET: api/Theses/discipline/{discipline}
        [HttpGet("discipline/{discipline}")]
        public async Task<ActionResult<object>> GetThesesByDiscipline(string discipline)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(discipline))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lấy dữ liệu thất bại",
                        error = "Discipline không được để trống"
                    });
                }

                var theses = await _context.Theses
                    .Where(t => t.Discipline.Contains(discipline))
                    .Include(t => t.Document)
                    .Select(t => new
                    {
                        t.DocumentID,
                        t.DegreeLevel,
                        t.Discipline,
                        t.AdvisorName,
                        //Document = new
                        //{
                        //    t.Document.ID,
                        //    t.Document.Title
                        //}
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Lấy danh sách luận văn chuyên ngành '{discipline}' thành công",
                    data = theses,
                    count = theses.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách luận văn theo chuyên ngành",
                    error = ex.Message
                });
            }
        }

        // GET: api/Theses/advisor/{advisorName}
        [HttpGet("advisor/{advisorName}")]
        public async Task<ActionResult<object>> GetThesesByAdvisor(string advisorName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(advisorName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lấy dữ liệu thất bại",
                        error = "Advisor Name không được để trống"
                    });
                }

                var theses = await _context.Theses
                    .Where(t => t.AdvisorName.Contains(advisorName))
                    .Include(t => t.Document)
                    .Select(t => new
                    {
                        t.DocumentID,
                        t.DegreeLevel,
                        t.Discipline,
                        t.AdvisorName,
                        //Document = new
                        //{
                        //    t.Document.ID,
                        //    t.Document.Title
                        //}
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Lấy danh sách luận văn của giảng viên '{advisorName}' thành công",
                    data = theses,
                    count = theses.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách luận văn theo giảng viên hướng dẫn",
                    error = ex.Message
                });
            }
        }

        // POST: api/Theses
        [HttpPost]
        public async Task<ActionResult<object>> PostThesis(ThesisCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.DocumentID))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo luận văn thất bại",
                        error = "Document ID không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.DegreeLevel))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo luận văn thất bại",
                        error = "Degree Level không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.Discipline))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo luận văn thất bại",
                        error = "Discipline không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.AdvisorName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo luận văn thất bại",
                        error = "Advisor Name không được để trống"
                    });
                }

                // Kiểm tra Document tồn tại
                var documentExists = await _context.Documents.AnyAsync(d => d.DocumentId == dto.DocumentID);
                if (!documentExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Tạo luận văn thất bại",
                        error = $"Tài liệu với ID {dto.DocumentID} không tồn tại"
                    });
                }

                // Kiểm tra luận văn đã tồn tại chưa
                var exists = await _context.Theses.AnyAsync(t => t.DocumentID == dto.DocumentID);
                if (exists)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Tạo luận văn thất bại",
                        error = $"Luận văn với Document ID {dto.DocumentID} đã tồn tại"
                    });
                }

                var thesis = new Thesis
                {
                    DocumentID = dto.DocumentID,
                    DegreeLevel = dto.DegreeLevel.Trim(),
                    Discipline = dto.Discipline.Trim(),
                    AdvisorName = dto.AdvisorName.Trim(),
                    Abstract = dto.Abstract?.Trim()
                };

                _context.Theses.Add(thesis);
                await _context.SaveChangesAsync();

                var result = await _context.Theses
                    .Include(t => t.Document)
                    .Where(t => t.DocumentID == thesis.DocumentID)
                    .Select(t => new
                    {
                        t.DocumentID,
                        t.DegreeLevel,
                        t.Discipline,
                        t.AdvisorName,
                        t.Abstract,
                        //Document = new
                        //{
                        //    t.Document.ID,
                        //    t.Document.Title
                        //}
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetThesis),
                    new { documentId = thesis.DocumentID },
                    new
                    {
                        success = true,
                        message = "Tạo luận văn thành công",
                        data = result
                    });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lưu luận văn vào database",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi tạo luận văn",
                    error = ex.Message
                });
            }
        }

        // PUT: api/Theses/{documentId}
        [HttpPut("{documentId}")]
        public async Task<ActionResult<object>> PutThesis(string documentId, ThesisUpdateDto dto)
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

                if (string.IsNullOrWhiteSpace(dto.DegreeLevel))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Degree Level không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.Discipline))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Discipline không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.AdvisorName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Advisor Name không được để trống"
                    });
                }

                var thesis = await _context.Theses.FindAsync(documentId);
                if (thesis == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = $"Luận văn với Document ID {documentId} không tồn tại"
                    });
                }

                thesis.DegreeLevel = dto.DegreeLevel.Trim();
                thesis.Discipline = dto.Discipline.Trim();
                thesis.AdvisorName = dto.AdvisorName.Trim();
                thesis.Abstract = dto.Abstract?.Trim();

                _context.Entry(thesis).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var result = await _context.Theses
                    .Include(t => t.Document)
                    .Where(t => t.DocumentID == documentId)
                    .Select(t => new
                    {
                        t.DocumentID,
                        t.DegreeLevel,
                        t.Discipline,
                        t.AdvisorName,
                        t.Abstract,
                        //Document = new
                        //{
                        //    t.Document.ID,
                        //    t.Document.Title
                        //}
                    })
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật luận văn thành công",
                    data = result
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Theses.AnyAsync(t => t.DocumentID == documentId))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Luận văn không tồn tại hoặc đã bị xóa"
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Xung đột dữ liệu. Luận văn có thể đã được cập nhật bởi người dùng khác"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi cập nhật luận văn",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/Theses/{documentId}
        [HttpDelete("{documentId}")]
        public async Task<ActionResult<object>> DeleteThesis(string documentId)
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

                var thesis = await _context.Theses.FindAsync(documentId);

                if (thesis == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Xóa thất bại",
                        error = $"Luận văn với Document ID {documentId} không tồn tại"
                    });
                }

                _context.Theses.Remove(thesis);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Xóa luận văn thành công",
                    deletedDocumentId = documentId
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Xóa thất bại",
                    error = "Không thể xóa luận văn. Có thể đang được sử dụng bởi dữ liệu khác",
                    details = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi xóa luận văn",
                    error = ex.Message
                });
            }
        }
    }
}