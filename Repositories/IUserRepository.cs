using System.Threading.Tasks;
using Schedify.Models;

namespace Schedify.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task CreateUserAsync(User user);
}