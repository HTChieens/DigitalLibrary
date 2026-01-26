using DigitalLibrary.Data;
using DigitalLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibrary.Repositories
{
    public class ReadingDocumentsRepository : IReadingDocumentRepository
    {
        private readonly DigitalLibraryContext _context;

        public ReadingDocumentsRepository(DigitalLibraryContext context)
        {
            this._context = context;
        }
        public async Task<bool> Add(ReadingDocument entity)
        {
            var result =await  this._context.ReadingDocuments.AddAsync(entity);
            this._context.SaveChanges();
            return result != null;
        }
        public async Task<bool> Delete(ReadingDocument entity)
        {
            var result = this._context.ReadingDocuments.Remove(entity);
            this._context.SaveChanges();
            return result != null;
        }

        public async Task<ReadingDocument> Find(string id)
        {
            var user =await this._context.ReadingDocuments.FirstOrDefaultAsync(u=>u.UserID==id);
            return user;
        }

        public async Task<ReadingDocument> Find(string userId, string documentId)
        {
            var reult = await this._context.ReadingDocuments
                .FirstOrDefaultAsync(r => r.UserID == userId && r.DocumentID == documentId);
            return reult;
        }

        public async Task<ICollection<ReadingDocument>> GetAllAsync()
        {
            return await _context.ReadingDocuments.ToListAsync();
        }

        public Task<int> GetMaxId()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Update(ReadingDocument entity)
        {
            var result = this._context.ReadingDocuments.Update(entity);
            this._context.SaveChanges();
            return result != null;
        }
    }
}
