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
    public class DocumentsController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public DocumentsController(DigitalLibraryContext context)
        {
            _context = context;
        }

        // GET: api/Documents
        [HttpGet]
        public async Task<ActionResult<object>> GetDocuments([FromQuery] bool includeDeleted = false)
        {
            try
            {
                var query = _context.Documents.AsQueryable();

                if (!includeDeleted)
                {
                    query = query.Where(d => !d.IsDeleted);
                }

                var documents = await query
                    .Select(d => new
                    {
                        d.ID,
                        d.Title,
                        d.Description,
                        d.DocumentType,
                        d.FilePath,
                        d.PublicationDate,
                        d.PageNum,
                        d.CreatedAt,
                        d.IntroEndPage,
                        d.CoverPath,
                        d.IsDeleted
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách tài liệu thành công",
                    data = documents,
                    count = documents.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách tài liệu",
                    error = ex.Message
                });
            }
        }

        // GET: api/Documents/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetDocument(string id)
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

                var document = await _context.Documents
                    .Include(d => d.Authors)
                    .Include(d => d.Doc_Keywords)
                        .ThenInclude(dk => dk.Keyword)
                    .Include(d => d.Document_Licenses)
                        .ThenInclude(dl => dl.License)
                    .Include(d => d.Identifiers)
                    .Include(d => d.Thesis)
                    .Include(d => d.ExternalBook)
                    .Include(d => d.InternalBook)
                    .Include(d => d.Research)
                    .Include(d => d.ResearchPublication)
                    .Where(d => d.ID == id)
                    .Select(d => new
                    {
                        d.ID,
                        d.Title,
                        d.Description,
                        d.DocumentType,
                        d.FilePath,
                        d.PublicationDate,
                        d.PageNum,
                        d.CreatedAt,
                        d.IntroEndPage,
                        d.CoverPath,
                        d.IsDeleted,
                        Authors = d.Authors.Select(a => new
                        {
                            a.ID,
                            a.Name,
                            a.Email
                        }).ToList(),
                        Keywords = d.Doc_Keywords.Select(dk => new
                        {
                            dk.Keyword.ID,
                            dk.Keyword.Name
                        }).ToList(),
                        Licenses = d.Document_Licenses.Select(dl => new
                        {
                            dl.License.ID,
                            dl.License.Name,
                            dl.AcceptedAt
                        }).ToList(),
                        Identifiers = d.Identifiers.Select(i => new
                        {
                            i.ID,
                            i.Type,
                            i.Value
                        }).ToList(),
                        Thesis = d.Thesis != null ? new
                        {
                            d.Thesis.DegreeLevel,
                            d.Thesis.Discipline,
                            d.Thesis.AdvisorName,
                            d.Thesis.Abstract
                        } : null,
                        ExternalBook = d.ExternalBook != null ? new
                        {
                            d.ExternalBook.Publisher,
                            d.ExternalBook.Version
                        } : null,
                        InternalBook = d.InternalBook != null ? new
                        {
                            d.InternalBook.Faculty,
                            d.InternalBook.DocumentType,
                            d.InternalBook.Version
                        } : null,
                        Research = d.Research != null ? new
                        {
                            d.Research.Abstract,
                            d.Research.ResearchLevel
                        } : null,
                        ResearchPublication = d.ResearchPublication != null ? new
                        {
                            d.ResearchPublication.VenueName,
                            d.ResearchPublication.PublicationType
                        } : null
                    })
                    .FirstOrDefaultAsync();

                if (document == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy tài liệu",
                        error = $"Tài liệu với ID {id} không tồn tại"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin tài liệu thành công",
                    data = document
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy thông tin tài liệu",
                    error = ex.Message
                });
            }
        }

        // GET: api/Documents/type/{documentType}
        [HttpGet("type/{documentType}")]
        public async Task<ActionResult<object>> GetDocumentsByType(string documentType, [FromQuery] bool includeDeleted = false)
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

                var query = _context.Documents
                    .Where(d => d.DocumentType == documentType);

                if (!includeDeleted)
                {
                    query = query.Where(d => !d.IsDeleted);
                }

                var documents = await query
                    .Select(d => new
                    {
                        d.ID,
                        d.Title,
                        d.Description,
                        d.DocumentType,
                        d.PublicationDate,
                        d.PageNum,
                        d.CoverPath,
                        d.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Lấy danh sách tài liệu loại '{documentType}' thành công",
                    data = documents,
                    count = documents.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách tài liệu theo loại",
                    error = ex.Message
                });
            }
        }

        // GET: api/Documents/search?keyword=abc
        [HttpGet("search")]
        public async Task<ActionResult<object>> SearchDocuments([FromQuery] string keyword, [FromQuery] bool includeDeleted = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tìm kiếm thất bại",
                        error = "Từ khóa tìm kiếm không được để trống"
                    });
                }

                var query = _context.Documents.AsQueryable();

                if (!includeDeleted)
                {
                    query = query.Where(d => !d.IsDeleted);
                }

                var documents = await query
                    .Where(d => d.Title.Contains(keyword) ||
                                (d.Description != null && d.Description.Contains(keyword)))
                    .Select(d => new
                    {
                        d.ID,
                        d.Title,
                        d.Description,
                        d.DocumentType,
                        d.PublicationDate,
                        d.CoverPath,
                        d.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Tìm thấy {documents.Count} tài liệu",
                    data = documents,
                    count = documents.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi tìm kiếm tài liệu",
                    error = ex.Message
                });
            }
        }

        // POST: api/Documents
        [HttpPost]
        public async Task<ActionResult<object>> PostDocument()
        {
            return StatusCode(501, new
            {
                success = false,
                message = "Chức năng tạo tài liệu chưa được triển khai"
            });
        }

        // PUT: api/Documents/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> PutDocument(string id, DocumentUpdateDto dto)
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

                if (string.IsNullOrWhiteSpace(dto.Title))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Title không được để trống"
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

                if (string.IsNullOrWhiteSpace(dto.FilePath))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "File Path không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.CoverPath))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Cover Path không được để trống"
                    });
                }

                if (dto.PageNum <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Page Number phải lớn hơn 0"
                    });
                }

                var document = await _context.Documents.FindAsync(id);
                if (document == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = $"Tài liệu với ID {id} không tồn tại"
                    });
                }

                document.Title = dto.Title.Trim();
                document.Description = dto.Description?.Trim();
                document.DocumentType = dto.DocumentType.Trim();
                document.FilePath = dto.FilePath.Trim();
                document.PublicationDate = dto.PublicationDate;
                document.PageNum = dto.PageNum;
                document.IntroEndPage = dto.IntroEndPage;
                document.CoverPath = dto.CoverPath.Trim();

                _context.Entry(document).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật tài liệu thành công",
                    data = new
                    {
                        document.ID,
                        document.Title,
                        document.Description,
                        document.DocumentType,
                        document.FilePath,
                        document.PublicationDate,
                        document.PageNum,
                        document.CreatedAt,
                        document.IntroEndPage,
                        document.CoverPath,
                        document.IsDeleted
                    }
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Documents.AnyAsync(d => d.ID == id))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Tài liệu không tồn tại hoặc đã bị xóa"
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Cập nhật thất bại",
                        error = "Xung đột dữ liệu. Tài liệu có thể đã được cập nhật bởi người dùng khác"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi cập nhật tài liệu",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/Documents/{id} (Soft delete)
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteDocument(string id)
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

                var document = await _context.Documents.FindAsync(id);

                if (document == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Xóa thất bại",
                        error = $"Tài liệu với ID {id} không tồn tại"
                    });
                }

                if (document.IsDeleted)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Xóa thất bại",
                        error = "Tài liệu đã bị xóa trước đó"
                    });
                }

                // Soft delete
                document.IsDeleted = true;
                _context.Entry(document).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Xóa tài liệu thành công (soft delete)",
                    deletedId = id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi xóa tài liệu",
                    error = ex.Message
                });
            }
        }

        // PUT: api/Documents/{id}/restore
        [HttpPut("{id}/restore")]
        public async Task<ActionResult<object>> RestoreDocument(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Khôi phục thất bại",
                        error = "ID không được để trống"
                    });
                }

                var document = await _context.Documents.FindAsync(id);

                if (document == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Khôi phục thất bại",
                        error = $"Tài liệu với ID {id} không tồn tại"
                    });
                }

                if (!document.IsDeleted)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Khôi phục thất bại",
                        error = "Tài liệu chưa bị xóa"
                    });
                }

                document.IsDeleted = false;
                _context.Entry(document).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Khôi phục tài liệu thành công",
                    restoredId = id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi khôi phục tài liệu",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/Documents/{id}/permanent
        [HttpDelete("{id}/permanent")]
        public async Task<ActionResult<object>> PermanentDeleteDocument(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Xóa vĩnh viễn thất bại",
                        error = "ID không được để trống"
                    });
                }

                var document = await _context.Documents
                    .Include(d => d.Collection_Documents)
                    .Include(d => d.Document_Licenses)
                    .Include(d => d.Downloads)
                    .Include(d => d.Identifiers)
                    .Include(d => d.ReadingDocuments)
                    .Include(d => d.Reviews)
                    .Include(d => d.SavedDocuments)
                    .Include(d => d.Submissions)
                    .Include(d => d.Doc_Keywords)
                    .Include(d => d.Thesis)
                    .Include(d => d.ExternalBook)
                    .Include(d => d.InternalBook)
                    .Include(d => d.Research)
                    .Include(d => d.ResearchPublication)
                    .FirstOrDefaultAsync(d => d.ID == id);

                if (document == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Xóa vĩnh viễn thất bại",
                        error = $"Tài liệu với ID {id} không tồn tại"
                    });
                }

                // Xóa tất cả dữ liệu liên quan
                _context.Collection_Documents.RemoveRange(document.Collection_Documents);
                _context.Document_Licenses.RemoveRange(document.Document_Licenses);
                _context.Downloads.RemoveRange(document.Downloads);
                _context.Identifiers.RemoveRange(document.Identifiers);
                _context.ReadingDocuments.RemoveRange(document.ReadingDocuments);
                _context.Reviews.RemoveRange(document.Reviews);
                _context.SavedDocuments.RemoveRange(document.SavedDocuments);
                _context.Submissions.RemoveRange(document.Submissions);
                _context.Doc_Keywords.RemoveRange(document.Doc_Keywords);

                if (document.Thesis != null)
                    _context.Theses.Remove(document.Thesis);
                if (document.ExternalBook != null)
                    _context.ExternalBooks.Remove(document.ExternalBook);
                if (document.InternalBook != null)
                    _context.InternalBooks.Remove(document.InternalBook);
                if (document.Research != null)
                    _context.Researches.Remove(document.Research);
                if (document.ResearchPublication != null)
                    _context.ResearchPublications.Remove(document.ResearchPublication);

                // Xóa document
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Xóa vĩnh viễn tài liệu thành công",
                    deletedId = id
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Xóa vĩnh viễn thất bại",
                    error = "Không thể xóa tài liệu. Có thể đang được sử dụng bởi dữ liệu khác",
                    details = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi xóa vĩnh viễn tài liệu",
                    error = ex.Message
                });
            }
        }
    }
}