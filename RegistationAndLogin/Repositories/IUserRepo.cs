using RegistationAndLogin.Domain.Entity;

namespace RegistationAndLogin.Repositories
{
    public interface IUserRepo
    {
        Task<User> GetByIdAsync(Guid userId);
        Task<User> GetByEmailAsync(string email);
        Task<User> CreateAsync(User user);

        Task<User> UpdateAsync(User user);
        Task<bool> EmailExistsAsync(string email);

    }
}
