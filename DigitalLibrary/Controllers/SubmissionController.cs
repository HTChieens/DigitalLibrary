using DigitalLibrary.Data;
using DigitalLibrary.DTOs.Librarians;
using DigitalLibrary.DTOs.Submissions;
using DigitalLibrary.Services.Submissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubmissionController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;
        private readonly ISubmissionService _submissionService;
        private readonly IEmailService _emailService;

        public SubmissionController(DigitalLibraryContext context, ISubmissionService submissionService, IEmailService emailService)
        {
            _context = context;
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
            try
            {
                await _submissionService.AssignReviewerAsync(submissionId, reviewerId, "4");
                return Ok(new { message = "Phân công thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("review")]
        public async Task<IActionResult> Review(ReviewSubmissionDto dto)
        {
            try
            {
                //var userId = User.FindFirst("id")!.Value;
                await _submissionService.ReviewAsync(dto, "10");
                return Ok(new { message = "Gửi đánh giá thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-assignments")]
        public async Task<IActionResult> GetMyAssignedSubmissions()
        {
            var userId = User.Identity?.Name;

            var submissions = await _submissionService.GetAssignedToReviewerAsync("5");
            return Ok(submissions);
        }

        [HttpPost("prereview")]
        public async Task<IActionResult> Prereview(Guid submissionId, string reviewerId)
        {
            try
            {
                var filePath = await _submissionService.PrereviewAsync(submissionId, reviewerId);
                return Ok(new
                {
                    submissionId,
                    filePath
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("finalreview")]
        public async Task<IActionResult> Approve(Guid id)
        {
            try
            {
                //var userId = User.FindFirst("id")!.Value;
                await _submissionService.FinalReviewAsync(id, "4");
                return Ok(new { message = "Phê duyệt thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        [HttpPost("adddoctocollection")]
        public async Task<IActionResult> AddDoctoCollectionAsync(AddDotoCollectionDto dto)
        {

            await _submissionService.AddDoctoCollectionAsync(dto);

            return Ok();
        }

        //[Authorize]
        [HttpGet("getsubmissionbyuser")]
        public async Task<IActionResult> MySubmissions()
        {
            //var userId = User.FindFirst("id")!.Value;
            return Ok(await _submissionService.GetByUserAsync("6"));
        }

        //[Authorize]
        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetHistory(Guid id)
        {
            var histories = await _submissionService.GetHistoryAsync(id);
            return Ok(histories.OrderByDescending(h => h.CreatedAt));
        }

        [HttpGet("info/{id}")]
        public async Task<IActionResult> GetInfo(Guid id)
        {
            var info = await _submissionService.GetSimpleInfoAsync(id);
            if (info == null) return NotFound(new { Message = "Submission not found" });
            return Ok(info);
        }

        //[Authorize(Roles = "Librarian")]
        [HttpGet("all-submissions")]
        public async Task<IActionResult> GetAll()
        {
            var submissions = await _submissionService.GetAllAsync();
            return Ok(submissions);
        }

        //[Authorize(Roles = "Librarian")]
        [HttpGet("lecturers")]
        public async Task<IActionResult> GetLecturers()
        {
            var lecturers = await _context.Users
                .Where(u => u.RoleID == "2")
                .Select(u => new { u.ID, u.Name, u.Email })
                .ToListAsync();
            return Ok(lecturers);
        }
    }
}
