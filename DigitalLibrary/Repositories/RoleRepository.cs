using DigitalLibrary.Data;
using DigitalLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibrary.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly DigitalLibraryContext _context;

        public RoleRepository(DigitalLibraryContext context)
        {
            this._context = context;
        }
        public async Task<bool> Add(Role entity)
        {
            var result =await  this._context.Roles.AddAsync(entity);
            this._context.SaveChanges();
            return result != null;
        }
        public async Task<bool> Delete(Role entity)
        {
            var result = this._context.Roles.Remove(entity);
            this._context.SaveChanges();
            return result != null;
        }

        public async Task<Role?> Find(string id)
        {
            var user =await this._context.Roles.FirstOrDefaultAsync(u=>u.ID==id);
            return user;
        }

        public async Task<ICollection<Role>> GetAllAsync()
        {
            return await _context.Roles.ToListAsync();
        }
        public async Task<int> GetMaxId()
        {
            return int.Parse(await _context.Roles.MaxAsync(u => u.ID));
        }
        public async Task<bool> Update(Role entity)
        {
            var result = this._context.Roles.Update(entity);
            this._context.SaveChanges();
            return result != null;
        }
    }
}
