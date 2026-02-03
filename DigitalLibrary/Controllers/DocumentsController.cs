using DigitalLibrary.Data;
using DigitalLibrary.DTOs;
using DigitalLibrary.DTOs.Documents;
using DigitalLibrary.DTOs.Submissions;
using DigitalLibrary.Models;
using DigitalLibrary.Services.Documents;
using DigitalLibrary.Services.Submissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;
        private readonly IDocumentService _documentService;
        private readonly ISubmissionService _submissionService;

        public DocumentsController(DigitalLibraryContext context, IDocumentService documentService, ISubmissionService submissionService)
        {
            _context = context;
            _documentService = documentService;
            _submissionService = submissionService;
        }

        // POST: api/documents/{documentId}/reviews
        // POST: api/documents/{documentId}/reviews
        [Authorize]
        [HttpPost("{documentId}/reviews")]
        public async Task<ActionResult<ReviewDto>> CreateReview(
            string documentId,
            [FromBody] CreateReviewDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                // Kiểm tra xem user đã đánh giá chưa
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.DocumentID == documentId && r.UserID == userId);

                if (existingReview != null)
                {
                    // **UPDATE** thay vì báo lỗi
                    existingReview.Rating = dto.Rating;
                    existingReview.Content = dto.Content?.Trim();
                    existingReview.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    return Ok(existingReview);
                };

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                // Kiểm tra document có tồn tại không
                var documentExists = await _context.Documents
                    .AnyAsync(d => d.DocumentId == documentId);

                if (!documentExists)
                {
                    return NotFound(new { message = "Tài liệu không tồn tại" });
                }

                // Tạo review mới
                var review = new Review
                {
                    DocumentID = documentId,
                    UserID = userId,
                    Rating = dto.Rating,
                    Content = dto.Content?.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                return Ok(review);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error creating review for document {DocumentId}", documentId);
                return StatusCode(500, new { message = "Lỗi khi tạo đánh giá: " + ex.Message });
            }
        }

        // PUT: api/documents/{documentId}/reviews/{reviewId}
        [Authorize]
        [HttpPut("{documentId}/reviews/{reviewId}")]
        public async Task<IActionResult> UpdateReview(
            string documentId,
            long reviewId,
            [FromBody] UpdateReviewDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var review = await _context.Reviews
                    .FirstOrDefaultAsync(r =>
                        r.ID == reviewId &&
                        r.DocumentID == documentId &&
                        r.UserID == userId);

                if (review == null)
                {
                    return NotFound("Không tìm thấy đánh giá hoặc bạn không có quyền chỉnh sửa");
                }

                // Cập nhật các trường
                if (dto.Rating.HasValue)
                {
                    review.Rating = dto.Rating.Value;
                }

                if (dto.Content != null)
                {
                    review.Content = dto.Content.Trim();
                }

                review.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error updating review {ReviewId}", reviewId);
                return StatusCode(500, "Lỗi khi cập nhật đánh giá");
            }
        }

        // DELETE: api/documents/{documentId}/reviews/{reviewId}
        [Authorize]
        [HttpDelete("{documentId}/reviews/{reviewId}")]
        public async Task<IActionResult> DeleteReview(string documentId, long reviewId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var review = await _context.Reviews
                    .FirstOrDefaultAsync(r =>
                        r.ID == reviewId &&
                        r.DocumentID == documentId &&
                        r.UserID == userId);

                if (review == null)
                {
                    return NotFound("Không tìm thấy đánh giá hoặc bạn không có quyền xóa");
                }

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error deleting review {ReviewId}", reviewId);
                return StatusCode(500, "Lỗi khi xóa đánh giá");
            }
        }

        // GET: api/documents/{documentId}/reviews/stats
        [HttpGet("{documentId}/reviews/stats")]
        public async Task<ActionResult<object>> GetReviewStats(string documentId)
        {
            try
            {
                var stats = await _context.Reviews
                    .Where(r => r.DocumentID == documentId && r.Rating.HasValue)
                    .GroupBy(r => 1)
                    .Select(g => new
                    {
                        TotalReviews = g.Count(),
                        AverageRating = g.Average(r => r.Rating.Value),
                        Rating5 = g.Count(r => r.Rating == 5),
                        Rating4 = g.Count(r => r.Rating == 4),
                        Rating3 = g.Count(r => r.Rating == 3),
                        Rating2 = g.Count(r => r.Rating == 2),
                        Rating1 = g.Count(r => r.Rating == 1)
                    })
                    .FirstOrDefaultAsync();

                if (stats == null)
                {
                    return Ok(new
                    {
                        TotalReviews = 0,
                        AverageRating = 0.0,
                        Rating5 = 0,
                        Rating4 = 0,
                        Rating3 = 0,
                        Rating2 = 0,
                        Rating1 = 0
                    });
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error getting review stats for document {DocumentId}", documentId);
                return StatusCode(500, "Lỗi khi lấy thống kê đánh giá");
            }
        }
        /// <summary>
        /// Lấy file PDF của tài liệu (inline, không cho download trực tiếp từ reader)
        /// </summary>
        [HttpGet("{id}/file")]
        public async Task<IActionResult> GetDocumentFile(string id)
        {
            try
            {
                var document = await _context.Documents
                    .Include(d => d.Files)
                    .FirstOrDefaultAsync(d => d.DocumentId == id && !d.IsDeleted);

                if (document == null)
                    return NotFound(new { message = "Tài liệu không tồn tại" });

                var documentFile = document.Files
                    .OrderByDescending(f => f.Version)
                    .FirstOrDefault();

                if (documentFile == null)
                    return NotFound(new { message = "Không tìm thấy file tài liệu" });

                // Xây dựng đường dẫn file thực tế
                //var filePath = Path.Combine(_env.WebRootPath, documentFile.FilePath.Replace("assets/", ""));
                var filePath = documentFile.FilePath;
                if (!System.IO.File.Exists(filePath))
                {
                    //_logger.LogError("File not found at path: {FilePath}", filePath);
                    return NotFound(new { message = "File không tồn tại trên server" });
                }

                // Đọc file
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                // ===== THÊM CORS HEADERS EXPLICITLY =====
                Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:4200");
                Response.Headers.Add("Access-Control-Allow-Credentials", "true");
                Response.Headers.Add("Access-Control-Allow-Methods", "GET, OPTIONS");
                Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                // Set headers để ngăn download trực tiếp, chỉ cho phép view inline
                Response.Headers.Add("Content-Disposition", "inline; filename=\"document.pdf\"");
                Response.Headers.Add("X-Content-Type-Options", "nosniff");
                Response.Headers.Add("Cache-Control", "private, max-age=3600"); // Cache 1 hour

                return File(fileBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error serving document file for document {DocumentId}", id);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi tải file" });
            }
        }

        /// <summary>
        /// Download tài liệu (yêu cầu đăng nhập)
        /// </summary>
        [Authorize]
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadDocument(string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var document = await _context.Documents
                    .Include(d => d.Files)
                    .FirstOrDefaultAsync(d => d.DocumentId == id && !d.IsDeleted);

                if (document == null)
                    return NotFound(new { message = "Tài liệu không tồn tại" });

                var documentFile = document.Files
                    .OrderByDescending(f => f.Version)
                    .FirstOrDefault();

                if (documentFile == null)
                    return NotFound(new { message = "Không tìm thấy file tài liệu" });

                //var filePath = Path.Combine(_env.WebRootPath, documentFile.FilePath.Replace("assets/", ""));
                var filePath= documentFile.FilePath;
                if (!System.IO.File.Exists(filePath))
                    return NotFound(new { message = "File không tồn tại trên server" });

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                // Tạo tên file an toàn
                var downloadFileName = $"{DateTime.Now.ToString()}.pdf";

                // Ghi nhận download
                //await RecordDownloadAsync(userId, id);

                // Set headers cho download
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{downloadFileName}\"");

                return File(fileBytes, "application/pdf", downloadFileName);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error downloading document {DocumentId}", id);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi tải tài liệu" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? authorId,
            [FromQuery] string? collectionId,
            [FromQuery] string? communityId,
            [FromQuery] string? type,
            [FromQuery] string? keyword,
            [FromQuery] string? sortBy = "newest",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            var result = await _documentService.GetAllAsync(
            authorId,
            collectionId,
            communityId,
            type,
            keyword,
            sortBy,
            page,
            pageSize
            );

            return Ok(result);
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(string id)
        {
            var doc = await _documentService.GetByIdAsync(id);
            if (doc == null)
            {
                return NotFound(new { message = "Document not found!" });
            }

            return Ok(doc);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string keyword)
        {
            var docs = await _documentService.SearchAsync(keyword);
            return Ok(docs);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateDocumentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var docId = await _documentService.CreateAsync(dto);

            var userId = User.Identity!.Name!;
            var submissionDto = new CreateSubmissionDto
            {
                DocumentId = docId,
                CollectionId = dto.CollectionId
            };
            await _submissionService.CreateAsync(submissionDto, "6");
            return Ok(new { documentId = docId });
        }

        [HttpGet("{id}/files")]
        public async Task<IActionResult> GetFilesByDocumentId(string id)
        {
            var file = await _documentService.GetFilesById(id);
            return Ok(file);
        }

        [HttpPost("upload-new-file")]
        public async Task<IActionResult> UploadNewFile(string docId, UploadNewFileDto dto)
        {
            var userId = User.Identity!.Name!;

            await _documentService.UploadNewVersionAsync(docId, dto, "6");

            return Ok();
        }


        [HttpPut("update/{submissionId}")]
        public async Task<IActionResult> Update(Guid submissionId, [FromForm] UpdateDocumentDto dto)
        {

            try
            {
                await _documentService.UpdateAsync(submissionId, dto, "6");
                return Ok(new { message = "Update successful" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("licenses")]
        public async Task<IActionResult> GetAllLicenses()
        {
            var licenses = await _context.Licenses
                .Select(l => new { l.ID, l.Name, l.Content })
                .ToListAsync();
            return Ok(licenses);
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularByDownload()
        {
            var data = await _documentService.GetByDownloadsAsync();
            return Ok(data);
        }

        [HttpGet("trending")]
        public async Task<IActionResult> GetTrendingByViews()
        {
            var data = await _documentService.GetByViewsAsync();
            return Ok(data);
        }


        [HttpGet("{documentId}/reviews")]
        public async Task<IActionResult> GetReviews(string documentId)
        {
            // Lấy userId từ JWT token (nếu user đã đăng nhập)
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var reviews = await _documentService.GetReviews(documentId, currentUserId);
            return Ok(reviews);
        }

        [HttpGet("communities")]
        public async Task<IActionResult> GetCommunities()
        {
            return Ok(await _documentService.GetCommunities());
        }

        [HttpGet("collections")]
        public async Task<IActionResult> GetCollections()
        {
            return Ok(await _documentService.GetCollections());
        }

        [HttpGet("authors")]
        public async Task<IActionResult> GetAuthors()
        {
            return Ok(await _documentService.GetAuthors());
        }

    }
}