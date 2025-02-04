using Microsoft.EntityFrameworkCore;

namespace FinanceManagerAPI.Infrastructure.Persistence {
    public class ApplicationDbContext : DbContext {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            if (!optionsBuilder.IsConfigured) {
                optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=finance_manager;Username=admin;Password=secret");
            }
        }
    }
}
