using DigitalLibrary.Models;

namespace DigitalLibrary.Repositories
{
    public interface IAuthorRepository : Repository<Author>
    {
        Task<ICollection<Document>> GetDocuments(string  authorId);
    }
}
