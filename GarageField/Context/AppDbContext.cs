using GarageField.Entities;
using Microsoft.EntityFrameworkCore;

namespace GarageField.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Tablolarımız
        public DbSet<Inspection> Inspections { get; set; }
        public DbSet<InspectionFile> InspectionFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurations klasöründeki ayarları (Fluent API) otomatik yükler
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}