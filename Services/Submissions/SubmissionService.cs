using DigitalLibrary.Data;
using DigitalLibrary.DTOs.Submissions;
using DigitalLibrary.Models;
using DigitalLibrary.Services.SubmissionHistories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibrary.Services.Submissions
{
    public class SubmissionService : ISubmissionService
    {
        private readonly DigitalLibraryContext _context;
        private readonly ISubmissionHistoryService _historyService;
        private readonly IEmailService _emailService;

        public SubmissionService(DigitalLibraryContext context, ISubmissionHistoryService historyService, IEmailService emailService)
        {
            _context = context;
            _historyService = historyService;
            _emailService = emailService;
        }

        public async Task<Guid> CreateAsync(CreateSubmissionDto dto, string submitterId)
        {
            var user = _context.Users.Where(u => u.Id == submitterId).FirstOrDefault();
            var role = _context.Roles.Where(r => r.Id == user!.RoleId).FirstOrDefault();
            bool isLibrarian = "Librarian" == role!.Name;

            var submission = new Submission
            {
                Id = Guid.NewGuid(),
                DocumentId = dto.DocumentId,
                CollectionId = dto.CollectionId,
                SubmitterId = submitterId,
                Status = isLibrarian ? "Accept" : "Submitt",
                CurrentStep = 1,
                CreatedAt = DateTime.UtcNow
            };

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            await _historyService.AddAsync(
                submission.Id,
                submitterId,
                isLibrarian ? "Accept" : "Submit",
                isLibrarian ? "Librarian submitted and accepted directly" : "Initial submission");


            if (!isLibrarian)
            {
                var emails = _context.Users.Where(u => u.RoleId == "3").Select(u => u.Email);
                foreach (string email in emails)
                {
                    await _emailService.SendAsync(
                        email,
                        "Có tài liệu mới được tải lên",
                        $@"
                        <p>Xin chào <b> thủ thư</b>,</p>
                        <p>
                            User có ID {user!.Id} đã tải lên tài liệu: <b>{submission.Document.Title}</b> với Submission ID là: {submission.Id}
                        </p>
                        <p>
                            Vui lòng vào website thư viện để phân Reviewer cho tài liệu này!
                        </p>
                    "
                    );
                }
            }

            await _emailService.SendAsync(
                user!.Email,
                "Có tài liệu mới được tải lên",
                $@"
                <p>Xin chào <b> {user.Name}</b>,</p>
                <p>
                    Bạn đã tải lên tài liệu: <b>{submission.Document.Title}</b> với Submission ID là: {submission.Id}
                </p>
                <p>
                    Vui lòng chờ đợi kết quả và sử dụng thông tin này để sử dụng cho việc phản hồi review (nếu có)!
                </p>
                "
            );

            return submission.Id;
        }

        public async Task<string> PrereviewAsync(Guid submissionId, string reviewerId)
        {
            var isAssigned = await _context.SubmissionHistories.AnyAsync(
                h => h.SubmissionId == submissionId
                && h.Action == "AssignReviewer"
                && h.Comment == reviewerId
            );
            if (!isAssigned)
                throw new Exception("You're not assigned to this submission");

            var submission = await _context.Submissions
                .Include(s => s.Document)
                .FirstOrDefaultAsync(s => s.Id == submissionId);
            if (submission!.Status == "Accept" || submission.Status == "Reject")
                throw new Exception("Submission is not in reviewable state");

            if (_context.SubmissionHistories.Any(sh => sh.PerformedBy == reviewerId && (sh.Comment == "OK" || sh.Comment == "Không đạt")))
            {
                throw new Exception("You have already reviewed the submission");
            }


            var last_review = _context.SubmissionHistories
                .Where(sh => sh.SubmissionId == submissionId)
                .OrderByDescending(sh => sh.CreatedAt)
                .First();
            if (last_review.PerformedBy == reviewerId)
            {
                throw new Exception("Author hasn’t had time to reconsider document yet");
            }

            return _context.DocumentFiles
                .Where(d => d.DocumentId == submission.DocumentId)
                .OrderByDescending(d => d.Version)
                .Select(d => d.FilePath)
                .First();
        }

        public async Task ReviewAsync(ReviewSubmissionDto dto, string reviewerId)
        {
            var submission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.Id == dto.SubmissionId);

            submission!.CurrentStep++;
            submission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _historyService.AddAsync(
                submission.Id,
                reviewerId,
                "Review",
                dto.Comment);

            var total = _context.SubmissionHistories.Where(sh => sh.SubmissionId == submission.Id && sh.Action == "AssignReviewer").Count();
            var count = _context.SubmissionHistories.Where(sh => sh.SubmissionId == submission.Id && sh.Comment == "OK" || sh.Comment == "Không đạt").Count();
            var emails = _context.Users.Where(u => u.RoleId == "4").Select(u => u.Email);
            if (count == total)
            {
                foreach (string email in emails)
                {
                    await _emailService.SendAsync(
                        email,
                        "Phê duyệt submission",
                        $@"
                        <p>Xin chào <b> thủ thư</b>,</p>
                        <p>
                            Các Reviewer đá đánh giá xong tài liệu {submission.Id}.
                        </p>
                        <p>
                            Vui lòng vào website thư viện để phê duyệt cho submission này!
                        </p>
                    "
                    );
                }
            }
        }

        public async Task FinalReviewAsync(Guid submissionId, string librarianId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            var submission = await _context.Submissions
                .Include(s => s.Document)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
            {
                throw new Exception("Submission not found");
            }

            var accept = _context.SubmissionHistories.Where(sh => sh.Comment == "OK").Count();
            var reject = _context.SubmissionHistories.Where(sh => sh.Comment == "Không đạt").Count();

            if (accept > reject)
            {
                submission.Status = "Accept";
            }
            else
            {
                submission.Status = "Reject";
            }

            submission.UpdatedAt = DateTime.UtcNow;

            _context.CollectionDocuments.Add(new CollectionDocument
            {
                CollectionId = submission.CollectionId,
                DocumentId = submission.DocumentId,
                AddedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            await _historyService.AddAsync(submission.Id, librarianId, "Accept", "Submitted by librarian");

            await tx.CommitAsync();
        }

        private int CalculateRequiredReviewers(int pageNum)
        {
            if (pageNum <= 50) return 1;
            if (pageNum <= 100) return 2;
            if (pageNum <= 200) return 3;
            return 4;
        }


        public async Task<int> GetAssignedReviewerCountAsync(Guid submissionId)
        {
            return await _context.SubmissionHistories
                .Where(h => h.SubmissionId == submissionId && h.Action == "AssignReviewer")
                .Select(h => h.Comment!.Trim())
                .Distinct()
                .CountAsync();
        }


        public async Task AssignReviewerAsync(Guid submissionId, string reviewerId, string librarianId)
        {
            var submission = await _context.Submissions.Include(s => s.Document).FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
            {
                throw new Exception("Submission not found");
            }

            if (submission.Status == "Accept" || submission.Status == "Reject")
                throw new Exception("Submission is not in review state");

            var reviewer = await _context.Users.FirstOrDefaultAsync(u => u.Id == reviewerId);
            if (reviewerId == null)
            {
                throw new Exception("User not found");
            }

            var alreadyAssigned = await _context.SubmissionHistories
                .AnyAsync(h => h.SubmissionId == submissionId && h.Action == "AssignReviewer" && h.Comment == reviewerId);
            if (alreadyAssigned)
                throw new Exception("This reviewer has already been assigned to this submission");

            var required = CalculateRequiredReviewers(submission.Document.PageNum);
            var currentAssigned = await _context.SubmissionHistories
                .Where(h => h.SubmissionId == submissionId && h.Action == "AssignReviewer")
                .Select(h => h.Comment)
                .Distinct()
                .CountAsync();
            if (currentAssigned >= required)
                throw new Exception("Reviewer limit reached for this document");


            submission.Status = "Review";

            await _historyService.AddAsync(submissionId, librarianId, "AssignReviewer", $"{reviewerId}");

            await _emailService.SendAsync(
                reviewer!.Email!,
                "Yêu cầu phê duyệt tài liệu",
                $@"
                    <p>Xin chào <b>{reviewer.Name}</b>,</p>
                    <p>
                        Bạn được phân công phê duyệt tài liệu: <b>{submission.Document.Title}</b> với Submission ID là: {submission.Id}
                    </p>
                    <p>
                        Vui lòng vào website thư viện để Review!
                    </p>
                "
            );

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid submissionId, Guid collectionId, string userId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            var submission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                throw new Exception("Submission not found");

            if (submission.Status == "Accept" || submission.Status == "Reject")
                throw new Exception("This submission cannot change collection");

            submission.CollectionId = collectionId;
            submission.UpdatedAt = DateTime.UtcNow;

            await _historyService.AddAsync(
                submissionId,
                userId,
                "Update",
                "Change collection");

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }

    }
}
