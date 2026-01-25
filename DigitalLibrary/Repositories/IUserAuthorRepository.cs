using DigitalLibrary.DTOs;
using DigitalLibrary.DTOs.UserAuthors;
using DigitalLibrary.Models;

namespace DigitalLibrary.Repositories
{
    public interface IUserAuthorRepository : Repository<User_Author>
    {
        Task<User_Author?> Find(UserAuthorDto dto);
    }
}
