namespace DigitalLibrary.Services.SubmissionHistories
{
    public interface ISubmissionHistoryService
    {
        Task AddAsync(Guid submissionId, string performedBy, string action, string? comment = null);
    }
}
