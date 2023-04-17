using JWT_Practice.Model;
using Microsoft.EntityFrameworkCore;

namespace JWT_Practice.DaraBase
{
    public class JwtDbContext : DbContext
    {
        public JwtDbContext(DbContextOptions<JwtDbContext> options) : base(options)
        {
        }

        public DbSet<Login> Logins { get; set; }
    }
}