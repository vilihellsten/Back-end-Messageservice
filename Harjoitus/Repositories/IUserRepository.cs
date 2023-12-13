using Harjoitus.Models;

namespace Harjoitus.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetUsersAsync();

        Task<User?> GetUserAsync(long id);

        Task<User?> GetUserAsync(string username);

        Task<User> NewUserAsync(User user);

        Task<bool> UpdateUserAsync(User user);

        Task<bool> DeleteUserAsync(User user);

    }

}
