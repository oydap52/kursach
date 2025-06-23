using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FurnitureProduction.Models;

namespace FurnitureProduction.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<OrderMaterial> OrderMaterials { get; set; }
        public DbSet<FurnitureTypeConfig> FurnitureTypeConfigs { get; set; }
        public DbSet<FurnitureMaterialTemplate> FurnitureMaterialTemplates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>().Property(o => o.TotalCost)
                .HasColumnType("decimal(12,2)")
                .HasPrecision(12, 2);

            modelBuilder.Entity<Order>().Property(o => o.Weight)
                .HasColumnType("decimal(10,2)")
                .HasPrecision(10, 2);

            modelBuilder.Entity<Material>().Property(m => m.UnitPrice)
                .HasColumnType("decimal(10,2)")
                .HasPrecision(10, 2);

            modelBuilder.Entity<Material>().Property(m => m.Weight)
                .HasColumnType("decimal(10,2)")
                .HasPrecision(10, 2);

            modelBuilder.Entity<FurnitureTypeConfig>().Property(f => f.BaseCost)
                .HasColumnType("decimal(12,2)")
                .HasPrecision(12, 2);

            modelBuilder.Entity<FurnitureTypeConfig>().Property(f => f.BaseWeight)
                .HasColumnType("decimal(10,2)")
                .HasPrecision(10, 2);

            modelBuilder.Entity<Employee>().Property(e => e.Salary)
                .HasColumnType("decimal(12,2)")
                .HasPrecision(12, 2);

            modelBuilder.Entity<FurnitureMaterialTemplate>()
                .Property(t => t.MinQuantity)
                .HasColumnType("DECIMAL(10, 2)");

            modelBuilder.Entity<FurnitureMaterialTemplate>()
                .Property(t => t.MaxQuantity)
                .HasColumnType("DECIMAL(10, 2)");

            // Тестовые данные для Employees
            modelBuilder.Entity<Employee>().HasData(
                new Employee { Id = 1, FullName = "Иванов Иван Иванович", Position = "Менеджер", Department = "Производство", Salary = 50000M, HireDate = DateTime.Now },
                new Employee { Id = 2, FullName = "Петров Пётр Петрович", Position = "Рабочий", Department = "Сборка", Salary = 40000M, HireDate = DateTime.Now }
            );

            // Тестовые данные для FurnitureMaterialTemplates
            modelBuilder.Entity<FurnitureMaterialTemplate>().HasData(
                new FurnitureMaterialTemplate { Id = 1, FurnitureType = "Диван", Category = "Древесина", IsRequired = true, MinQuantity = 1.0M, MaxQuantity = 2.0M },
                new FurnitureMaterialTemplate { Id = 2, FurnitureType = "Диван", Category = "Ткань", IsRequired = true, MinQuantity = 2.0M, MaxQuantity = 4.0M },
                new FurnitureMaterialTemplate { Id = 3, FurnitureType = "Диван", Category = "Наполнитель", IsRequired = true, MinQuantity = 1.0M, MaxQuantity = 3.0M },
                new FurnitureMaterialTemplate { Id = 4, FurnitureType = "Диван", Category = "Фурнитура", IsRequired = true, MinQuantity = 4.0M, MaxQuantity = 8.0M },
                new FurnitureMaterialTemplate { Id = 5, FurnitureType = "Диван", Category = "Отделка", IsRequired = false, MinQuantity = 0.0M, MaxQuantity = 1.0M }
            );
        }
    }
}