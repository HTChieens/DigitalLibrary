using DigitalLibrary.Data;
using DigitalLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibrary.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly DigitalLibraryContext _context;

        public PermissionRepository(DigitalLibraryContext context)
        {
            this._context = context;
        }
        public async Task<bool> Add(Permission entity)
        {
            var result =await  this._context.Permissions.AddAsync(entity);
            this._context.SaveChanges();
            return result != null;
        }
        public async Task<bool> Delete(Permission entity)
        {
            var result = this._context.Permissions.Remove(entity);
            this._context.SaveChanges();
            return result != null;
        }

        public async Task<Permission?> Find(string id)
        {
            var result =await this._context.Permissions.FirstOrDefaultAsync(u=>u.ID==id);
            return result;
        }

        public async Task<ICollection<Permission>> GetAllAsync()
        {
            return await _context.Permissions.ToListAsync();
        }
        public async Task<int> GetMaxId()
        {
            return int.Parse(await _context.Permissions.MaxAsync(u => u.ID));
        }
        public async Task<bool> Update(Permission entity)
        {
            var result = this._context.Permissions.Update(entity);
            this._context.SaveChanges();
            return result != null;
        }
    }
}
