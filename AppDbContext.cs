using FurnitureProduction.Models;
using Microsoft.EntityFrameworkCore;

namespace FurnitureProduction.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
    }
}