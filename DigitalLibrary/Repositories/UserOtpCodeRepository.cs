using DigitalLibrary.Data;
using DigitalLibrary.DTOs.Documents;
using DigitalLibrary.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;

namespace DigitalLibrary.Repositories
{
    public class UserOtpCodeRepository : IUserOtpCodeRepository
    {
        private readonly DigitalLibraryContext _context;

        public UserOtpCodeRepository(DigitalLibraryContext context)
        {
            this._context = context;
        }
        public async Task<bool> Add(UserOtpCode userOtpCode)
        {
           var result =  await _context.UserOtpCode.AddAsync(userOtpCode);
            await _context.SaveChangesAsync();
            return result!=null;
        }

        public async Task<UserOtpCode> GetByUserId(string userId)
        {
            return await _context.UserOtpCode.Where(u => u.UserId == userId)
                .OrderByDescending(u=>u.ExpiredAt)
                .FirstOrDefaultAsync();
        } 

        public async Task<bool> Update(UserOtpCode userOtpCode)
        {
            var result = _context.UserOtpCode.Update(userOtpCode);
            await _context.SaveChangesAsync();
            return result !=null;
        }
    }
}
