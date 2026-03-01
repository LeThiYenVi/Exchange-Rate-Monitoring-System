using ExchangeRate.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRate.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Models.ExchangeRate> ExchangeRates { get; set; }
        public DbSet<WorkerStatus> WorkerStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Models.ExchangeRate>(entity =>
            {
                entity.ToTable("ExchangeRates");
                entity.Property(e => e.Rate).HasColumnType("decimal(18,4)");
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.CurrencyCode);
            });

            modelBuilder.Entity<WorkerStatus>(entity =>
            {
                entity.ToTable("WorkerStatuses");
            });
        }
    }
}
