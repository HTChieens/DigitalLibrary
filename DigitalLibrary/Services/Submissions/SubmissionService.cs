using DigitalLibrary.Data;
using DigitalLibrary.DTOs.Librarians;
using DigitalLibrary.DTOs.Submissions;
using DigitalLibrary.Models;
using DigitalLibrary.Services.SubmissionHistories;
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
            var user = _context.Users.Where(u => u.ID == submitterId).FirstOrDefault();
            var role = _context.Roles.Where(r => r.ID == user!.RoleID).FirstOrDefault();
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
                var emails = _context.Users.Where(u => u.RoleID == "3").Select(u => u.Email);
                foreach (string email in emails)
                {
                    await _emailService.SendAsync(
                        email,
                        "Có tài liệu mới được tải lên",
                        $@"
                        <p>Xin chào <b> thủ thư</b>,</p>
                        <p>
                            User có ID {user!.ID} đã tải lên tài liệu: <b>{submission.Document.Title}</b> với Submission ID là: {submission.Id}
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

            if (submission == null) throw new Exception("Submission not found");
            if (submission.Status == "Accept" || submission.Status == "Reject")
                throw new Exception("Submission is not in reviewable state");

            var lastAction = await _context.SubmissionHistories
                .Where(sh => sh.SubmissionId == submissionId)
                .OrderByDescending(sh => sh.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastAction != null && lastAction.PerformedBy == reviewerId)
            {
                throw new Exception("Author hasn’t had time to reconsider document yet");
            }

            var alreadyReviewed = await _context.SubmissionHistories.AnyAsync(sh =>
                sh.SubmissionId == submissionId &&
                sh.PerformedBy == reviewerId &&
                (sh.Comment == "OK" || sh.Comment == "Không đạt")
            );

            if (alreadyReviewed)
            {
                throw new Exception("You have already reviewed this version of the submission");
            }

            var file = await _context.DocumentFiles
                .Where(d => d.DocumentId == submission.DocumentId)
                .OrderByDescending(d => d.Version)
                .FirstOrDefaultAsync();

            if (file == null) throw new Exception("No document file found");

            return file.FilePath;
        }

        public async Task ReviewAsync(ReviewSubmissionDto dto, string reviewerId)
        {
            var submission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.Id == dto.SubmissionId);

            if (submission == null) throw new Exception("Không tìm thấy Submission");

            submission.CurrentStep++;
            submission.UpdatedAt = DateTime.UtcNow;

            await _historyService.AddAsync(
                submission.Id,
                reviewerId,
                "Review",
                dto.Comment);

            await _context.SaveChangesAsync();

            var total = await _context.SubmissionHistories
                .CountAsync(sh => sh.SubmissionId == submission.Id && sh.Action == "AssignReviewer");

            var count = await _context.SubmissionHistories
                .CountAsync(sh => sh.SubmissionId == submission.Id && (sh.Comment == "OK" || sh.Comment == "Không đạt"));

            if (count == total)
            {
                var librarians = await _context.Users.Where(u => u.RoleID == "3").ToListAsync();
                foreach (var lib in librarians)
                {
                    await _emailService.SendAsync(
                        lib.Email!,
                        "Tài liệu đã hoàn tất quá trình Review",
                        $"Tài liệu <b>{submission.Document.Title}</b> đã được tất cả các Reviewer đánh giá. Vui lòng thực hiện phê duyệt cuối cùng."
                    );
                }
            }
        }

        public async Task<List<SubmissionListDto>> GetAssignedToReviewerAsync(string reviewerId)
        {
            var assignedSubmissionIds = await _context.SubmissionHistories
                .Where(h => h.Action == "AssignReviewer" && h.Comment == reviewerId)
                .Select(h => h.SubmissionId)
                .Distinct()
                .ToListAsync();

            return await _context.Submissions
                .Where(s => assignedSubmissionIds.Contains(s.Id))
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new SubmissionListDto
                {
                    SubmissionId = s.Id,
                    DocumentTitle = s.Document.Title,
                    DocumentType = s.Document.DocumentType,
                    CollectionName = s.Collection.Name,
                    CreatedAt = s.CreatedAt,
                    Status = s.Status,
                    CurrentStep = s.CurrentStep,

                    ReviewerCount = _context.SubmissionHistories
                        .Where(h => h.SubmissionId == s.Id && h.Action == "AssignReviewer")
                        .Select(h => h.Comment)
                        .Distinct()
                        .Count()
                })
                .ToListAsync();
        }

        public async Task FinalReviewAsync(Guid submissionId, string librarianId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            var submission = await _context.Submissions
                .Include(s => s.Document)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null) throw new Exception("Submission not found");

            var assignedReviewerIds = await _context.SubmissionHistories
                .Where(h => h.SubmissionId == submissionId && h.Action == "AssignReviewer")
                .Select(h => h.Comment)
                .ToListAsync();

            if (assignedReviewerIds.Count == 0)
                throw new Exception("Tài liệu này chưa được phân công Reviewer nào.");

            var reviews = await _context.SubmissionHistories
                .Where(h => h.SubmissionId == submissionId && h.Action == "Review")
                .ToListAsync();

            var reviewerWhoFinished = reviews.Select(r => r.PerformedBy).Distinct().Count();

            if (reviewerWhoFinished < assignedReviewerIds.Count)
            {
                throw new Exception($"Chưa thể phê duyệt. Mới có {reviewerWhoFinished}/{assignedReviewerIds.Count} Reviewer hoàn thành đánh giá.");
            }

            var acceptCount = reviews.Count(r => r.Comment == "OK");
            var rejectCount = reviews.Count(r => r.Comment == "Không đạt");

            string finalStatus;
            if (acceptCount > rejectCount)
            {
                finalStatus = "Accept";
                _context.CollectionDocuments.Add(new CollectionDocument
                {
                    CollectionId = submission.CollectionId,
                    DocumentId = submission.DocumentId,
                    AddedAt = DateTime.UtcNow
                });
            }
            else
            {
                finalStatus = "Reject";
            }

            submission.Status = finalStatus;
            submission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _historyService.AddAsync(submission.Id, librarianId, finalStatus, $"Thủ thư phê duyệt cuối cùng dựa trên {acceptCount} phiếu thuận / {rejectCount} phiếu chống.");

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

            var reviewer = await _context.Users.FirstOrDefaultAsync(u => u.ID == reviewerId);
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

        public async Task UpdateAsync(Guid submissionId, Guid? collectionId, string userId, bool hasNewFile)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            var submission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.Id == submissionId)
                ?? throw new Exception("Submission not found");

            if (submission.Status is "Accept" or "Reject")
                throw new Exception("This submission cannot be modified");

            if (collectionId.HasValue && submission.CollectionId != collectionId)
            {
                submission.CollectionId = collectionId.Value;

                await _historyService.AddAsync(
                    submissionId,
                    userId,
                    "Update",
                    "Change collection");
            }

            if (hasNewFile)
            {
                submission.Status = "Pending";
                submission.CurrentStep += 1;

                await _historyService.AddAsync(
                    submissionId,
                    userId,
                    "Revise",
                    "Submit new version");
            }
            else
            {
                await _historyService.AddAsync(
                    submissionId,
                    userId,
                    "Revise",
                    "Update document information");
            }

            submission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }


        public async Task AddDoctoCollectionAsync(AddDotoCollectionDto dto)
        {
            _context.CollectionDocuments.Add(new CollectionDocument
            {
                CollectionId = dto.CollectionId,
                DocumentId = dto.DocumentId,
                AddedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        public async Task<List<SubmissionListDto>> GetByUserAsync(string userId)
        {
            return await _context.Submissions
                .Where(s => s.SubmitterId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new SubmissionListDto
                {
                    SubmissionId = s.Id,
                    DocumentTitle = s.Document.Title,
                    DocumentType = s.Document.DocumentType,
                    CollectionName = s.Collection.Name,
                    CreatedAt = s.CreatedAt,
                    Status = s.Status,

                    CurrentStep = s.CurrentStep,

                    ReviewerCount = _context.SubmissionHistories
                        .Where(h =>
                            h.SubmissionId == s.Id &&
                            h.Action == "AssignReviewer"
                        )
                        .Select(h => h.Comment)
                        .Distinct()
                        .Count()
                })
                .ToListAsync();
        }

        public async Task<List<SubmissionListDto>> GetAllAsync()
        {
            return await _context.Submissions
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new SubmissionListDto
                {
                    SubmissionId = s.Id,
                    DocumentTitle = s.Document.Title,
                    DocumentType = s.Document.DocumentType,
                    CollectionName = s.Collection.Name,
                    CreatedAt = s.CreatedAt,
                    Status = s.Status,

                    CurrentStep = s.CurrentStep,

                    ReviewerCount = _context.SubmissionHistories
                        .Where(h =>
                            h.SubmissionId == s.Id &&
                            h.Action == "AssignReviewer"
                        )
                        .Select(h => h.Comment)
                        .Distinct()
                        .Count()
                })
                .ToListAsync();
        }



        public async Task<List<SubmissionHistoryDto>> GetHistoryAsync(Guid submissionId)
        {
            var histories = await _context.SubmissionHistories
                .Where(h => h.SubmissionId == submissionId)
                .OrderBy(h => h.CreatedAt)
                .ToListAsync();

            var userIds = histories
                .Select(h => h.PerformedBy)
                .Concat(histories
                    .Where(h => h.Action.Equals("AssignReviewer", StringComparison.OrdinalIgnoreCase))
                    .Select(h => h.Comment))
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.ID))
                .ToDictionaryAsync(u => u.ID, u => u.Name);

            return histories.Select(h =>
            {
                string? comment = h.Comment;

                if (h.Action.Equals("AssignReviewer", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrEmpty(h.Comment)
                    && users.TryGetValue(h.Comment, out var reviewerName))
                {
                    comment = $"Reviewer: {reviewerName}";
                }

                return new SubmissionHistoryDto
                {
                    Id = h.Id,
                    Action = h.Action,
                    Comment = comment,
                    PerformedById = h.PerformedBy,
                    PerformedByName = users.GetValueOrDefault(h.PerformedBy, "Unknown"),
                    CreatedAt = h.CreatedAt
                };
            }).ToList();
        }


        public async Task<object?> GetSimpleInfoAsync(Guid submissionId)
        {
            return await _context.Submissions
                .Where(s => s.Id == submissionId)
                .Select(s => new
                {
                    Id = s.Id,
                    DocumentId = s.DocumentId,
                    SubmitterId = s.SubmitterId,
                    Status = s.Status,
                    CollectionId = s.CollectionId
                })
                .FirstOrDefaultAsync();
        }
    }
}
