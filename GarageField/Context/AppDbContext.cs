using GarageField.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GarageField.Data;

public class AppDbContext : DbContext
{
    public DbSet<Inspection> Inspections { get; set; }
    public DbSet<InspectionFile> InspectionFiles { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<Inspection>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<InspectionFile>().HasQueryFilter(x => !x.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries().ToList();

        foreach (var entry in entries.Where(e => e.Entity is Inspection && e.State == EntityState.Modified))
        {
            var inspection = (Inspection)entry.Entity;

            if (inspection.IsDeleted && (bool)entry.Property(nameof(ISoftDelete.IsDeleted)).OriginalValue == false)
            {
                var files = await InspectionFiles
                    .Where(f => f.InspectionId == inspection.Id && !f.IsDeleted)
                    .ToListAsync(cancellationToken);

                foreach (var file in files)
                {
                    file.IsDeleted = true;
                    file.DeletedAt = DateTime.UtcNow;
                    InspectionFiles.Update(file);
                }
            }
        }

        foreach (var entry in entries.Where(e => e.Entity is InspectionFile))
        {
            var file = (InspectionFile)entry.Entity;

            if (file.IsDeleted && (entry.State == EntityState.Modified || entry.State == EntityState.Deleted))
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    file.IsDeleted = true;
                    file.DeletedAt = DateTime.UtcNow;
                }

                var remainingFilesCount = await InspectionFiles
                    .CountAsync(f => f.InspectionId == file.InspectionId && f.Id != file.Id && !f.IsDeleted, cancellationToken);

                if (remainingFilesCount == 0)
                {
                    var inspection = await Inspections.FindAsync(new object[] { file.InspectionId }, cancellationToken);
                    if (inspection != null && !inspection.IsDeleted)
                    {
                        inspection.IsDeleted = true;
                        inspection.DeletedAt = DateTime.UtcNow;
                        inspection.UpdatedAt = DateTime.UtcNow;
                        Inspections.Update(inspection);
                    }
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}