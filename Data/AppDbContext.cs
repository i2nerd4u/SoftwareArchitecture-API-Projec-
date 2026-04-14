using Microsoft.EntityFrameworkCore;
using Software_architecture_api.Models;

namespace Software_architecture_api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Item> Items => Set<Item>();
        public DbSet<Game> Games => Set<Game>();
    }
}