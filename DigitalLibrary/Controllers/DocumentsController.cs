using DigitalLibrary.Data;
using DigitalLibrary.DTOs;
using DigitalLibrary.DTOs.Documents;
using DigitalLibrary.DTOs.Submissions;
using DigitalLibrary.Models;
using DigitalLibrary.Services.Documents;
using DigitalLibrary.Services.Submissions;
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
	    private readonly IDocumentService _documentService;
        private readonly ISubmissionService _submissionService;

        public DocumentsController(DigitalLibraryContext context , IDocumentService documentService, ISubmissionService submissionService)
        {
            _context = context;
	_documentService = documentService;
            _submissionService = submissionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var docs = await _documentService.GetAllAsync();
            return Ok(docs);
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
    }
}