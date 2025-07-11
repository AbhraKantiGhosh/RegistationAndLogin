using Microsoft.EntityFrameworkCore;
using RegistationAndLogin.Domain.Entity;

namespace RegistationAndLogin
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
       
    }
}
