using DigitalLibrary.Data;
using DigitalLibrary.DTOs.Documents;
using DigitalLibrary.DTOs.Submissions;
using DigitalLibrary.Services.Documents;
using DigitalLibrary.Services.Submissions;
using Microsoft.AspNetCore.Mvc;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ISubmissionService _submissionService;

        public DocumentsController(DigitalLibraryContext context, IDocumentService documentService, ISubmissionService submissionService)
        {
            _documentService = documentService;
            _submissionService = submissionService;
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
        public async Task<IActionResult> Create(CreateDocumentDto dto)
        {
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

        [HttpPut("update")]
        public async Task<IActionResult> Update(Guid submissionId, UpdateDocumentDto dto)
        {
            await _documentService.UpdateAsync(submissionId, dto);

            var userId = User.Identity!.Name!;

            await _submissionService.UpdateAsync(submissionId, dto.CollectionId, "6");
            return Ok();
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

        [HttpGet("{id}/reviews")]
        public async Task<IActionResult> GetReviews(string id)
        {
            var reviews = await _documentService.GetReviews(id);
            return Ok(reviews);
        }

        [HttpGet("communities")]
        public async Task<IActionResult> GetCommunities()
        {
            return Ok(await _documentService.GetCommunities());
        }

        // 2. Lấy danh sách Bộ sưu tập
        [HttpGet("collections")]
        public async Task<IActionResult> GetCollections()
        {
            return Ok(await _documentService.GetCollections());
        }

        // 3. Lấy danh sách Tác giả
        [HttpGet("authors")]
        public async Task<IActionResult> GetAuthors()
        {
            return Ok(await _documentService.GetAuthors());
        }

    }
}