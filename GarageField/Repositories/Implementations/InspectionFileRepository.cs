using EF.Core.Repository.Repository;
using EFCore.BulkExtensions;
using GarageField.Data;
using GarageField.Entities;
using GarageField.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GarageField.Repositories.Implementations;

public class InspectionFileRepository : CommonRepository<InspectionFile>, IInspectionFileRepository
{
    private readonly AppDbContext _context;

    public InspectionFileRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<InspectionFile>> GetByInspectionIdAsync(Guid inspectionId)
    {
        return await _context.InspectionFiles
            .Where(f => f.InspectionId == inspectionId)
            .ToListAsync();
    }

    public async Task<List<InspectionFile>> GetAllAsync()
    {
        return await _context.InspectionFiles.ToListAsync();
    }

    public async Task<InspectionFile?> GetByIdAndInspectionIdAsync(Guid inspectionId, Guid fileId)
    {
        return await _context.InspectionFiles
            .FirstOrDefaultAsync(x => x.Id == fileId && x.InspectionId == inspectionId);
    }

    public async Task InsertManyAsync(List<InspectionFile> inspectionFiles)
    {
        await _context.BulkInsertAsync(inspectionFiles);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(InspectionFile file)
    {
        _context.InspectionFiles.Remove(file);
        await _context.SaveChangesAsync();
    }
}