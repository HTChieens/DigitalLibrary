using DigitalLibrary.Data;
using DigitalLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibrary.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DigitalLibraryContext _context;

        public  UserRepository(DigitalLibraryContext context)
        {
            this._context = context;
        }
        public async Task<bool> Add(User entity)
        {
            var result =await  this._context.Users.AddAsync(entity);
            this._context.SaveChanges();
            return result != null;
        }
        public async Task<bool> Delete(User entity)
        {
            var result = this._context.Users.Remove(entity);
            this._context.SaveChanges();
            return result != null;
        }

        public async Task<User?> Find(string id)
        {
            var user =await this._context.Users.FirstOrDefaultAsync(u=>u.ID==id);
            return user;
        }

        public async Task<ICollection<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<Author>? GetAuthor(string userId)
        {
            var user_author = await _context.User_Authors.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user_author == null) return null;
            return await _context.Authors.FindAsync(user_author.AuthorID);
        }

        public async Task<int> GetMaxId()
        {
            return int.Parse(await _context.Users.MaxAsync(u => u.ID));
        }

        public async Task<bool> Update(User entity)
        {
            var result = this._context.Users.Update(entity);
            this._context.SaveChanges();
            return result != null;
        }
    }
}
