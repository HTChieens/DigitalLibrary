using DigitalLibrary.Data;
using DigitalLibrary.DTOs;
using DigitalLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop.Infrastructure;

namespace DigitalLibrary.Repositories
{
    public class UserAuthorRepository : IUserAuthorRepository
    {
        private readonly DigitalLibraryContext _context;

        public UserAuthorRepository(DigitalLibraryContext context)
        {
            this._context = context;
        }
        public async Task<bool> Add(User_Author entity)
        {
            var result =await  this._context.User_Authors.AddAsync(entity);
            this._context.SaveChanges();
            return result != null;
        }
        public async Task<bool> Delete(User_Author entity)
        {
            var result = this._context.User_Authors.Remove(entity);
            this._context.SaveChanges();
            return result != null;
        }

        public async Task<User_Author?> Find(string id)
        {
            //to do
            var user =await this._context.User_Authors.FirstOrDefaultAsync(u=>u.UserID==id);
            return user;
        }

        public async Task<User_Author?> Find(UserAuthorDto dto)
        {
            var user_author = await this._context.User_Authors.FirstOrDefaultAsync(u=>u.UserID == dto.UserId && u.AuthorID==dto.AuthorId);
            return user_author;
        }

        public async Task<ICollection<User_Author>> GetAllAsync()
        {
            return await _context.User_Authors.ToListAsync();
        }
        //not use
        public async Task<int> GetMaxId()
        {
            //to do
            return int.Parse(await _context.User_Authors.MaxAsync(u => u.UserID));
        }

        public async Task<bool> Update(User_Author entity)
        {
            var result = this._context.User_Authors.Update(entity);
            this._context.SaveChanges();
            return result != null;
        }
    }
}
