using AuthenticationDemo.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationDemo.Data
{
    public class UserDbContext(DbContextOptions<UserDbContext>  option) : DbContext(option)
    {
        public DbSet<User> Users { get; set; }

    }
}
