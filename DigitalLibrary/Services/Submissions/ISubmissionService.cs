using DigitalLibrary.DTOs.Documents;
using DigitalLibrary.DTOs.Librarians;
using DigitalLibrary.DTOs.Submissions;
using Microsoft.AspNetCore.Mvc;

namespace DigitalLibrary.Services.Submissions
{
    public interface ISubmissionService
    {
        Task<Guid> CreateAsync(CreateSubmissionDto dto, string submitterId);
        Task ReviewAsync(ReviewSubmissionDto dto, string reviewerId);
        Task<string> PrereviewAsync(Guid submissionId, string reviewerId);
        Task FinalReviewAsync(Guid submissionId, string librarianId);
        Task AssignReviewerAsync(Guid submissionId, string lecturerId, string librarianId);
        Task UpdateAsync(Guid submissionId, UpdateDocumentDto dto, string userId);
        Task AddDoctoCollectionAsync(AddDotoCollectionDto dto);
    }
}
