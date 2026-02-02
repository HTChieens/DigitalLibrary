using DigitalLibrary.DTOs.Librarians;
using DigitalLibrary.DTOs.Submissions;

namespace DigitalLibrary.Services.Submissions
{
    public interface ISubmissionService
    {
        Task<Guid> CreateAsync(CreateSubmissionDto dto, string submitterId);
        Task ReviewAsync(ReviewSubmissionDto dto, string reviewerId);
        Task<string> PrereviewAsync(Guid submissionId, string reviewerId);
        Task FinalReviewAsync(Guid submissionId, string librarianId);
        Task AssignReviewerAsync(Guid submissionId, string lecturerId, string librarianId);
        Task UpdateAsync(Guid submissionId, Guid? collectionId, string userId, bool hasNewFile);
        Task AddDoctoCollectionAsync(AddDotoCollectionDto dto);
        Task<List<SubmissionListDto>> GetByUserAsync(string userId);
        Task<List<SubmissionHistoryDto>> GetHistoryAsync(Guid submissionId);
        Task<object?> GetSimpleInfoAsync(Guid submissionId);
        Task<List<SubmissionListDto>> GetAllAsync();
        Task<List<SubmissionListDto>> GetAssignedToReviewerAsync(string reviewerId);
    }
}
