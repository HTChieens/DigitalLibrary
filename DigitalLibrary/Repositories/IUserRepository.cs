using DigitalLibrary.Models;

namespace DigitalLibrary.Repositories
{
    public interface IUserRepository: Repository<User>
    {
       Task< Author> GetAuthor(string userId);
       Task<User> GetByEmail(string email);
    }
}
