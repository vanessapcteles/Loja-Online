
using LojaOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace LojaOnline.Data
{
    public class ApiDbContext : DbContext
    {
        // O construtor que recebe as opções de configuração
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        {
        }

        // Define as tuas tabelas (DbSet<Tabela>)
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        // Configura relações
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define que o SKU na tabela Products deve ser único
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Sku)
                .IsUnique();

            // Define que o Username e Email na tabela Users devem ser únicos
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
