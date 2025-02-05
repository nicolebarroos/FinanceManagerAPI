using FinanceManagerAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

//ApplicationDbContext gerencia a conexão com o banco usando o Entity Framework Core.
//Cada modelo precisa ser registrado com DbSet<T>.
//Configura relacionamentos (One-to-Many, Many-to-Many) e regras (Email único).
//A string de conexão é configurada no Program.cs para conectar ao PostgreSQL.

namespace FinanceManagerAPI.Infrastructure.Persistence {
    public class ApplicationDbContext : DbContext {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
