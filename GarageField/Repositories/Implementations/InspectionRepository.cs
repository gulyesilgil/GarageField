using EF.Core.Repository.Repository;
using EFCore.BulkExtensions;
using GarageField.Data;
using GarageField.Entities;
using GarageField.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GarageField.Repositories.Implementations;

public class InspectionRepository : CommonRepository<Inspection>, IInspectionRepository
{
    private readonly AppDbContext _context;

    public InspectionRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Inspection?> GetByIdAsync(Guid id)
    {
        return await _context.Inspections.FindAsync(id);
    }

    public async Task<List<Inspection>> GetAllAsync()
    {
        return await _context.Inspections.ToListAsync();
    }

    public async Task InsertManyAsync(List<Inspection> inspections)
    {
        // 🚀 ÇÖZÜM: BulkInsert zaten yazar, arkasındaki SaveChangesAsync kaldırıldı!
        await _context.BulkInsertAsync(inspections);
    }

    public new void Delete(Inspection inspection)
    {
        inspection.IsDeleted = true;
        inspection.DeletedAt = DateTime.UtcNow;

        // 🚀 ÇÖZÜM: Buradaki erken SaveChanges() uçuruldu, yetki tamamen servis katmanına devredildi!
        _context.Inspections.Update(inspection);
    }
}