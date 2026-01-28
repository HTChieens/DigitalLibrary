using DigitalLibrary.DTOs.Librarians;
using DigitalLibrary.DTOs.Submissions;
using DigitalLibrary.Services.Submissions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubmissionController : ControllerBase
    {
        private readonly ISubmissionService _submissionService;
        private readonly IEmailService _emailService;

        public SubmissionController(ISubmissionService submissionService, IEmailService emailService)
        {
            _submissionService = submissionService;
            _emailService = emailService;
        }
        
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateSubmissionDto dto)
        {
            var userId = User.Identity!.Name!;
            var id = await _submissionService.CreateAsync(dto, userId);
            return Ok(new { submissionId = id });
        }

        [HttpPost("assign-reviewer")]
        public async Task<IActionResult> AssignReviewer(Guid submissionId, string reviewerId)
        {
            var librarianId = User.Identity!.Name!;

            await _submissionService.AssignReviewerAsync(submissionId, reviewerId, "4");

            return Ok();
        }

        [HttpPost("review")]
        public async Task<IActionResult> Review(ReviewSubmissionDto dto)
        {
            await _submissionService.ReviewAsync(dto, "6");
            return Ok();
        }

        [HttpPost("prereview")]
        public async Task<IActionResult> Prereview(Guid submissionId, string reviewerId)
        {
            var filePath = await _submissionService.PrereviewAsync(submissionId, reviewerId);

            return Ok(new
            {
                submissionId,
                filePath
            });
        }


        [HttpPost("finalreview")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var librarianId = User.Identity!.Name!;
            await _submissionService.FinalReviewAsync(id, "4");
            return Ok();
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateCollection(Guid submissionId, Guid collectionId)
        {
            var userId = User.Identity!.Name!;

            await _submissionService.UpdateAsync(submissionId, collectionId, "4");

            return Ok();
        }

        [HttpPost("adddoctocollection")]
        public async Task<IActionResult> AddDoctoCollectionAsync(AddDotoCollectionDto dto)
        {

            await _submissionService.AddDoctoCollectionAsync(dto);

            return Ok();
        }
    }
}
