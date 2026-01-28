using DigitalLibrary.DTOs.Documents;

namespace DigitalLibrary.Services.Documents
{
    public interface IDocumentService
    {
        Task<List<DocumentListDto>> GetAllAsync();
        Task<DocumentDetailDto?> GetByIdAsync(string Id);
        Task<List<DocumentListDto>> SearchAsync(string keyword);
        Task<string> CreateAsync(CreateDocumentDto dto);
        Task UploadNewVersionAsync(UploadNewFileDto dto, string userId);
        Task UpdateAsync(Guid submissionId, UpdateDocumentDto dto);
    }
}
