using fast_authenticator.model.tiers;
using fast_authenticator.model;
using Microsoft.EntityFrameworkCore;
using fast_auth.model.tiers;

namespace fast_authenticator.context
{
    public class MyDbContext : DbContext
    {
        public DbSet<Status> Statuses { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Authentification> Authentifications { get; set; }
        public DbSet<UniqueKey> UniqueKeys { get; set; }
        public DbSet<ResetEmailRequest> ResetEmailRequests { get; set; }
        public DbSet<Token> Tokens { get; set; }

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Forcer les noms de colonnes en minuscule
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.GetColumnName().ToLower());
                }
            }
        }

    }

}
