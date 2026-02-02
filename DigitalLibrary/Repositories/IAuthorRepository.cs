using DigitalLibrary.DTOs.Documents;
using DigitalLibrary.Models;

namespace DigitalLibrary.Repositories
{
    public interface IAuthorRepository : Repository<Author>
    {
        Task<ICollection<DocumentListDto>> GetDocuments(string  authorId);
    }
}
