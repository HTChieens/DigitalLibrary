using DigitalLibrary.Models;

namespace DigitalLibrary.Repositories
{
    public interface IReadingDocumentRepository : Repository<ReadingDocument>
    {
        Task<ReadingDocument> Find(string userId, string documentId);
    }
}
