using DigitalLibrary.DTOs.Documents;
using DigitalLibrary.Models;

namespace DigitalLibrary.Repositories
{
    public interface IUserOtpCodeRepository
    {
        Task<bool> Add(UserOtpCode userOtpCode);
        Task<UserOtpCode> GetByUserId(string userId);
        Task<bool> Update(UserOtpCode userOtpCode);
    }
}
