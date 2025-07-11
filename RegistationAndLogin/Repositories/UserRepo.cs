using Microsoft.EntityFrameworkCore;
using RegistationAndLogin.Domain.Entity;

namespace RegistationAndLogin.Repositories
{
    public class UserRepo : IUserRepo
    {

        private readonly ApplicationDbContext _context;
        
        public UserRepo(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public Task<bool> EmailExistsAsync(string email)
        {
            return _context.Users.AnyAsync(u => u.Email == email.ToLower());
        }

        public Task<User?> GetByEmailAsync(string email)
        {
            return _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByIdAsync(Guid userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<User> UpdateAsync(User user)
        {
             _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
