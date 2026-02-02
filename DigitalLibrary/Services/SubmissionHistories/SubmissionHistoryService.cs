using DigitalLibrary.Data;
using DigitalLibrary.Models;

namespace DigitalLibrary.Services.SubmissionHistories
{
    public class SubmissionHistoryService : ISubmissionHistoryService
    {
        private readonly DigitalLibraryContext _context;

        public SubmissionHistoryService(DigitalLibraryContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Guid submissionId, string performedBy, string action, string? comment = null)
        {
            var history = new SubmissionHistory
            {
                Id = Guid.NewGuid(),
                SubmissionId = submissionId,
                PerformedBy = performedBy,
                Action = action,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.SubmissionHistories.Add(history);
            await _context.SaveChangesAsync();
        }
    }
}
