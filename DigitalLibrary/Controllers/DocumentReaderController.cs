using DigitalLibrary.Data;
using DigitalLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentReaderController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DocumentReaderController> _logger;

        public DocumentReaderController(
            DigitalLibraryContext context,
            IWebHostEnvironment env,
            ILogger<DocumentReaderController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Lấy thông tin quyền truy cập tài liệu
        /// </summary>
        [HttpGet("{documentId}/access-info")]
        public async Task<IActionResult> GetAccessInfo(string documentId)
        {
            try
            {
                var document = await _context.Documents
                    .Where(d => d.DocumentId == documentId && !d.IsDeleted)
                    .Select(d => new
                    {
                        d.DocumentId,
                        d.Title,
                        d.PageNum,
                        d.IntroEndPage
                    })
                    .FirstOrDefaultAsync();

                if (document == null)
                    return NotFound(new { message = "Tài liệu không tồn tại" });

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                bool isAuthenticated = !string.IsNullOrEmpty(userId);

                int? lastReadPage = null;
                if (isAuthenticated)
                {
                    var readingDoc = await _context.ReadingDocuments
                        .Where(rd => rd.UserID == userId && rd.DocumentID == documentId)
                        .Select(rd => rd.CurrentPage)
                        .FirstOrDefaultAsync();

                    lastReadPage = readingDoc;
                }

                return Ok(new
                {
                    documentId = document.DocumentId,
                    title = document.Title,
                    totalPages = document.PageNum,
                    introEndPage = document.IntroEndPage ?? 0,
                    isAuthenticated = isAuthenticated,
                    lastReadPage = lastReadPage,
                    canDownload = isAuthenticated
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access info for document {DocumentId}", documentId);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy thông tin tài liệu" });
            }
        }

        /// <summary>
        /// Lấy thông tin trang cụ thể (có kiểm tra quyền truy cập)
        /// </summary>
        [HttpGet("{documentId}/page/{pageNumber}")]
        public async Task<IActionResult> GetPage(string documentId, int pageNumber)
        {
            try
            {
                var document = await _context.Documents
                    .Include(d => d.Files)
                    .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);

                if (document == null)
                    return NotFound(new { message = "Tài liệu không tồn tại" });

                // Kiểm tra số trang hợp lệ
                if (pageNumber < 0 || pageNumber >= document.PageNum)
                {
                    return BadRequest(new { message = "Số trang không hợp lệ" });
                }

                // Kiểm tra quyền truy cập
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                bool isAuthenticated = !string.IsNullOrEmpty(userId);

                int maxPageForGuest = document.IntroEndPage ?? 0;

                // Nếu chưa đăng nhập, chỉ cho xem trang giới thiệu
                if (!isAuthenticated && pageNumber > maxPageForGuest)
                {
                    return Unauthorized(new
                    {
                        message = "Vui lòng đăng nhập để xem toàn bộ tài liệu",
                        maxPage = maxPageForGuest,
                        requiresAuth = true
                    });
                }

                var documentFile = document.Files
                    .OrderByDescending(f => f.Version)
                    .FirstOrDefault();

                if (documentFile == null)
                    return NotFound(new { message = "Không tìm thấy file tài liệu" });

                // Cập nhật lịch sử đọc nếu đã đăng nhập
                if (isAuthenticated)
                {
                    await UpdateReadingHistoryAsync(userId, documentId, pageNumber);
                }

                return Ok(new
                {
                    success = true,
                    filePath = documentFile.FilePath,
                    currentPage = pageNumber,
                    totalPages = document.PageNum,
                    maxPageForGuest = maxPageForGuest,
                    isAuthenticated = isAuthenticated
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting page {PageNumber} of document {DocumentId}",
                    pageNumber, documentId);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi tải trang" });
            }
        }

        /// <summary>
        /// Cập nhật tiến độ đọc (yêu cầu đăng nhập)
        /// </summary>
        [Authorize]
        [HttpPost("{documentId}/update-progress")]
        public async Task<IActionResult> UpdateProgress(
            string documentId,
            [FromBody] UpdateProgressRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Vui lòng đăng nhập" });

                // Kiểm tra document tồn tại
                var documentExists = await _context.Documents
                    .AnyAsync(d => d.DocumentId == documentId && !d.IsDeleted);

                if (!documentExists)
                    return NotFound(new { message = "Tài liệu không tồn tại" });

                await UpdateReadingHistoryAsync(userId, documentId, request.CurrentPage);

                return Ok(new { message = "Cập nhật tiến độ thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating progress for document {DocumentId}", documentId);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi cập nhật tiến độ" });
            }
        }

        /// <summary>
        /// Lấy lịch sử đọc của user (yêu cầu đăng nhập)
        /// </summary>
        [Authorize]
        [HttpGet("reading-history")]
        public async Task<IActionResult> GetReadingHistory(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var query = _context.ReadingDocuments
                    .Where(rd => rd.UserID == userId)
                    .Include(rd => rd.Document)
                        .ThenInclude(d => d.Authors)
                    .OrderByDescending(rd => rd.LastReadAt);

                var totalCount = await query.CountAsync();

                var history = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(rd => new
                    {
                        documentId = rd.DocumentID,
                        title = rd.Document.Title,
                        coverPath = rd.Document.CoverPath,
                        currentPage = rd.CurrentPage,
                        totalPages = rd.Document.PageNum,
                        progress = rd.Document.PageNum > 0
                            ? (int)((rd.CurrentPage + 1) * 100.0 / rd.Document.PageNum)
                            : 0,
                        lastReadAt = rd.LastReadAt,
                        firstReadAt = rd.FirstReadAt,
                        authors = rd.Document.Authors.Select(a => a.Name).ToList()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    data = history,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reading history");
                return StatusCode(500, new { message = "Có lỗi xảy ra" });
            }
        }

        /// <summary>
        /// Private method để cập nhật hoặc tạo mới reading history
        /// </summary>
        private async Task UpdateReadingHistoryAsync(string userId, string documentId, int currentPage)
        {
            var readingDoc = await _context.ReadingDocuments
                .FirstOrDefaultAsync(rd => rd.UserID == userId && rd.DocumentID == documentId);

            var now = DateTime.UtcNow;

            if (readingDoc == null)
            {
                // Tạo mới reading document
                readingDoc = new ReadingDocument
                {
                    UserID = userId,
                    DocumentID = documentId,
                    CurrentPage = currentPage,
                    FirstReadAt = now,
                    LastReadAt = now,
                    IsCounted = false
                };
                _context.ReadingDocuments.Add(readingDoc);

                // Tăng view count cho document (chỉ tính lần đầu)
                await IncrementViewCountAsync(documentId);
            }
            else
            {
                // Cập nhật reading document hiện có
                readingDoc.CurrentPage = currentPage;
                readingDoc.LastReadAt = now;
                _context.ReadingDocuments.Update(readingDoc);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Tăng view count cho document
        /// </summary>
        private async Task IncrementViewCountAsync(string documentId)
        {
            // Nếu bạn có trường TotalViews trong Document model
            // var document = await _context.Documents.FindAsync(documentId);
            // if (document != null)
            // {
            //     document.TotalViews++;
            //     await _context.SaveChangesAsync();
            // }

            // Hoặc sử dụng ExecuteUpdate (EF Core 7+)
            // await _context.Documents
            //     .Where(d => d.DocumentId == documentId)
            //     .ExecuteUpdateAsync(s => s.SetProperty(d => d.TotalViews, d => d.TotalViews + 1));
        }
    }

    /// <summary>
    /// Request model cho update progress
    /// </summary>
    public class UpdateProgressRequest
    {
        public int CurrentPage { get; set; }
    }
}