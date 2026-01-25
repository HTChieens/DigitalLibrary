using DigitalLibrary.Data;
using DigitalLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibrary.Repositories
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly DigitalLibraryContext _context;

        public AuthorRepository(DigitalLibraryContext context)
        {
            this._context = context;
        }
        public async Task<bool> Add(Author entity)
        {
            var result =await  this._context.Authors.AddAsync(entity);
            this._context.SaveChanges();
            return result != null;
        }
        public async Task<bool> Delete(Author entity)
        {
            var result = this._context.Authors.Remove(entity);
            this._context.SaveChanges();
            return result != null;
        }

        public async Task<Author?> Find(string id)
        {
            var user =await this._context.Authors.Include(au=>au.Documents).FirstOrDefaultAsync(u=>u.ID==id);
            return user;
        }

        public async Task<ICollection<Author>> GetAllAsync()
        {
            return await _context.Authors.ToListAsync();
        }

        public async Task<ICollection<Document>> GetDocuments(string authorId)
        {
            var author =await this.Find(authorId);
            if (author == null) return null;
            return author.Documents;
        }

        public async Task<int> GetMaxId()
        {
            return int.Parse(await _context.Authors.MaxAsync(u => u.ID));
        }

        public async Task<bool> Update(Author entity)
        {
            var result = this._context.Authors.Update(entity);
            this._context.SaveChanges();
            return result != null;
        }
    }
}
