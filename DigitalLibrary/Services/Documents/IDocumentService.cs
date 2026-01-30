using DigitalLibrary.DTOs.Documents;
using DigitalLibrary.Models;

namespace DigitalLibrary.Services.Documents
{
    public interface IDocumentService
    {
        Task<object> GetAllAsync(
            string? authorId,
            string? collectionId,
            string? communityId,
            string? type,
            string? keyword,
            string sortBy,
            int page,
            int pageSize
        );

        Task<DocumentDetailDto?> GetByIdAsync(string Id);
        Task<List<DocumentListDto>> SearchAsync(string keyword);
        Task<string> CreateAsync(CreateDocumentDto dto);
        Task UploadNewVersionAsync(string documentId, UploadNewFileDto dto, string userId);
        Task UpdateAsync(Guid submissionId, UpdateDocumentDto dto);
        Task<List<DocumentFile>> GetFilesById(string Id);
        Task<List<DocumentList2Dto>> GetByViewsAsync();
        Task<List<DocumentPopularDto>> GetByDownloadsAsync();
        Task<List<ReviewDto>> GetReviews(string id);

        Task<List<CommunityTreeDto>> GetCommunities();
        Task<List<Collection>> GetCollections();
        Task<List<Author>> GetAuthors();
    }
}
