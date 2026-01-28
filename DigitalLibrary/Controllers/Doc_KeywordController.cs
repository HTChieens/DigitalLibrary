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
    public class DocKeywordsController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public DocKeywordsController(DigitalLibraryContext context)
        {
            _context = context;
        }

        // GET: api/DocKeywords
        [HttpGet]
        public async Task<ActionResult<object>> GetDocKeywords()
        {
            try
            {
                var docKeywords = await _context.Doc_Keywords
                    .Include(dk => dk.Document)
                    .Include(dk => dk.Keyword)
                    .Select(dk => new
                    {
                        dk.DocumentID,
                        dk.KeywordID,
                        //Document = new
                        //{
                        //    dk.Document.ID,
                        //    dk.Document.Title
                        //},
                        //Keyword = new
                        //{
                        //    dk.Keyword.ID,
                        //    dk.Keyword.Name
                        //}
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách từ khóa tài liệu thành công",
                    data = docKeywords,
                    count = docKeywords.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách từ khóa tài liệu",
                    error = ex.Message
                });
            }
        }

        // GET: api/DocKeywords/document/{documentId}
        [HttpGet("document/{documentId}")]
        public async Task<ActionResult<object>> GetKeywordsByDocument(string documentId)
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

                var keywords = await _context.Doc_Keywords
                    .Where(dk => dk.DocumentID == documentId)
                    .Include(dk => dk.Keyword)
                    .Select(dk => new
                    {
                        dk.DocumentID,
                        dk.KeywordID,
                        Keyword = new
                        {
                            dk.Keyword.ID,
                            dk.Keyword.Name
                        }
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách từ khóa của tài liệu thành công",
                    data = keywords,
                    count = keywords.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách từ khóa của tài liệu",
                    error = ex.Message
                });
            }
        }

        // GET: api/DocKeywords/keyword/{keywordId}
        [HttpGet("keyword/{keywordId}")]
        public async Task<ActionResult<object>> GetDocumentsByKeyword(string keywordId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keywordId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lấy dữ liệu thất bại",
                        error = "Keyword ID không được để trống"
                    });
                }

                var keywordExists = await _context.Keywords.AnyAsync(k => k.ID == keywordId);
                if (!keywordExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy từ khóa",
                        error = $"Từ khóa với ID {keywordId} không tồn tại"
                    });
                }

                var documents = await _context.Doc_Keywords
                    .Where(dk => dk.KeywordID == keywordId)
                    .Include(dk => dk.Document)
                    .Select(dk => new
                    {
                        dk.DocumentID,
                        dk.KeywordID,
                        Document = new
                        {
                            dk.Document.ID,
                            dk.Document.Title
                        }
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách tài liệu theo từ khóa thành công",
                    data = documents,
                    count = documents.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách tài liệu theo từ khóa",
                    error = ex.Message
                });
            }
        }

        // GET: api/DocKeywords/{documentId}/{keywordId}
        [HttpGet("{documentId}/{keywordId}")]
        public async Task<ActionResult<object>> GetDocKeyword(string documentId, string keywordId)
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

                if (string.IsNullOrWhiteSpace(keywordId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Lấy dữ liệu thất bại",
                        error = "Keyword ID không được để trống"
                    });
                }

                var docKeyword = await _context.Doc_Keywords
                    .Include(dk => dk.Document)
                    .Include(dk => dk.Keyword)
                    .Where(dk => dk.DocumentID == documentId && dk.KeywordID == keywordId)
                    .Select(dk => new
                    {
                        dk.DocumentID,
                        dk.KeywordID,
                        Document = new
                        {
                            dk.Document.ID,
                            dk.Document.Title
                        },
                        Keyword = new
                        {
                            dk.Keyword.ID,
                            dk.Keyword.Name
                        }
                    })
                    .FirstOrDefaultAsync();

                if (docKeyword == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy quan hệ từ khóa - tài liệu",
                        error = $"Không tìm thấy quan hệ giữa tài liệu {documentId} và từ khóa {keywordId}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin từ khóa tài liệu thành công",
                    data = docKeyword
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy thông tin từ khóa tài liệu",
                    error = ex.Message
                });
            }
        }

        // POST: api/DocKeywords
        [HttpPost]
        public async Task<ActionResult<object>> PostDocKeyword(DocKeywordCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.DocumentID))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Thêm từ khóa thất bại",
                        error = "Document ID không được để trống"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.KeywordID))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Thêm từ khóa thất bại",
                        error = "Keyword ID không được để trống"
                    });
                }

                // Kiểm tra Document tồn tại
                var documentExists = await _context.Documents.AnyAsync(d => d.ID == dto.DocumentID);
                if (!documentExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Thêm từ khóa thất bại",
                        error = $"Tài liệu với ID {dto.DocumentID} không tồn tại"
                    });
                }

                // Kiểm tra Keyword tồn tại
                var keywordExists = await _context.Keywords.AnyAsync(k => k.ID == dto.KeywordID);
                if (!keywordExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Thêm từ khóa thất bại",
                        error = $"Từ khóa với ID {dto.KeywordID} không tồn tại"
                    });
                }

                // Kiểm tra quan hệ đã tồn tại chưa
                var exists = await _context.Doc_Keywords
                    .AnyAsync(dk => dk.DocumentID == dto.DocumentID && dk.KeywordID == dto.KeywordID);

                if (exists)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Thêm từ khóa thất bại",
                        error = "Tài liệu này đã có từ khóa này rồi"
                    });
                }

                var docKeyword = new Doc_Keyword
                {
                    DocumentID = dto.DocumentID,
                    KeywordID = dto.KeywordID
                };

                _context.Doc_Keywords.Add(docKeyword);
                await _context.SaveChangesAsync();

                // Lấy thông tin đầy đủ để trả về
                var result = await _context.Doc_Keywords
                    .Include(dk => dk.Document)
                    .Include(dk => dk.Keyword)
                    .Where(dk => dk.DocumentID == docKeyword.DocumentID &&
                                 dk.KeywordID == docKeyword.KeywordID)
                    .Select(dk => new
                    {
                        dk.DocumentID,
                        dk.KeywordID,
                        Document = new
                        {
                            dk.Document.ID,
                            dk.Document.Title
                        },
                        Keyword = new
                        {
                            dk.Keyword.ID,
                            dk.Keyword.Name
                        }
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetDocKeyword),
                    new { documentId = docKeyword.DocumentID, keywordId = docKeyword.KeywordID },
                    new
                    {
                        success = true,
                        message = "Thêm từ khóa cho tài liệu thành công",
                        data = result
                    });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lưu từ khóa tài liệu vào database",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi thêm từ khóa cho tài liệu",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/DocKeywords/{documentId}/{keywordId}
        [HttpDelete("{documentId}/{keywordId}")]
        public async Task<ActionResult<object>> DeleteDocKeyword(string documentId, string keywordId)
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

                if (string.IsNullOrWhiteSpace(keywordId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Xóa thất bại",
                        error = "Keyword ID không được để trống"
                    });
                }

                var docKeyword = await _context.Doc_Keywords
                    .FirstOrDefaultAsync(dk => dk.DocumentID == documentId && dk.KeywordID == keywordId);

                if (docKeyword == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Xóa thất bại",
                        error = "Không tìm thấy quan hệ từ khóa - tài liệu"
                    });
                }

                _context.Doc_Keywords.Remove(docKeyword);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Xóa từ khóa khỏi tài liệu thành công",
                    deletedDocumentId = documentId,
                    deletedKeywordId = keywordId
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Xóa thất bại",
                    error = "Không thể xóa từ khóa tài liệu",
                    details = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi xóa từ khóa tài liệu",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/DocKeywords/document/{documentId}
        [HttpDelete("document/{documentId}")]
        public async Task<ActionResult<object>> DeleteAllKeywordsFromDocument(string documentId)
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

                var docKeywords = await _context.Doc_Keywords
                    .Where(dk => dk.DocumentID == documentId)
                    .ToListAsync();

                if (!docKeywords.Any())
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy từ khóa nào",
                        error = $"Tài liệu {documentId} không có từ khóa nào"
                    });
                }

                _context.Doc_Keywords.RemoveRange(docKeywords);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Xóa tất cả từ khóa khỏi tài liệu thành công",
                    deletedCount = docKeywords.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi xóa từ khóa khỏi tài liệu",
                    error = ex.Message
                });
            }
        }
    }
}