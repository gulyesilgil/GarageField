using GarageField.DTOs.Inspection;
using GarageField.Entities;
using GarageField.Enums;
using GarageField.Repositories.Interfaces;
using GarageField.Services.StorageServices;
using GarageField.Data; // 🚀 AppDbContext için eklendi
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GarageField.Services.InspectionServices;

public class InspectionService
{
    private readonly IInspectionRepository _inspectionRepository;
    private readonly IInspectionFileRepository _fileRepository;
    private readonly IFileStorageService _storageService;
    private readonly AppDbContext _context; // 
    private readonly string _bucketName;

    public InspectionService(
        IInspectionRepository inspectionRepository,
        IInspectionFileRepository fileRepository,
        IFileStorageService storageService,
        AppDbContext context, // 
        IConfiguration config)
    {
        _inspectionRepository = inspectionRepository;
        _fileRepository = fileRepository;
        _storageService = storageService;
        _context = context;
        _bucketName = config["GarageSettings:BucketName"]!;
    }

    public async Task<InspectionDto> CreateInspectionAsync(CreateInspectionDto dto)
    {
        var entity = new Inspection
        {
            Id = Guid.NewGuid(), // 
            ProductName = dto.ProductName,
            Description = dto.Description,
            InspectorName = dto.InspectorName,
            Status = InspectionStatus.Pending,
            UpdatedAt = DateTime.UtcNow
        };

        await _inspectionRepository.InsertManyAsync(new List<Inspection> { entity });
        return ToDto(entity);
    }

    public async Task<List<InspectionDto>> CreateBulkInspectionsAsync(BulkCreateInspectionDto bulkDto)
    {
        var entityList = new List<Inspection>();

        foreach (var dto in bulkDto.Inspections)
        {
            var entity = new Inspection
            {
                Id = Guid.NewGuid(), // 
                ProductName = dto.ProductName,
                Description = dto.Description,
                InspectorName = dto.InspectorName,
                Status = InspectionStatus.Pending,
                UpdatedAt = DateTime.UtcNow
            };
            entityList.Add(entity);
        }

        await _inspectionRepository.InsertManyAsync(entityList);
        return entityList.Select(ToDto).ToList();
    }

    public async Task<List<InspectionDto>> GetAllInspectionsAsync()
    {
        var inspections = await _inspectionRepository.GetAllAsync();
        return inspections.Select(ToDto).ToList();
    }

    public async Task<InspectionDto?> GetInspectionByIdAsync(Guid id)
    {
        var inspection = await _inspectionRepository.GetByIdAsync(id);
        return inspection == null ? null : ToDto(inspection);
    }

    public async Task<bool> UpdateAsync(Guid id, CreateInspectionDto dto)
    {
        var inspection = await _inspectionRepository.GetByIdAsync(id);
        if (inspection == null) return false;

        inspection.ProductName = dto.ProductName;
        inspection.Description = dto.Description;
        inspection.InspectorName = dto.InspectorName;
        inspection.UpdatedAt = DateTime.UtcNow;

        await _inspectionRepository.InsertManyAsync(new List<Inspection> { inspection });
        return true;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, string status)
    {
        var inspection = await _inspectionRepository.GetByIdAsync(id);
        if (inspection == null) return false;

        if (Enum.TryParse<InspectionStatus>(status, true, out var parsed))
        {
            inspection.Status = parsed;
            inspection.UpdatedAt = DateTime.UtcNow;
            await _inspectionRepository.InsertManyAsync(new List<Inspection> { inspection });
            return true;
        }
        return false;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var inspection = await _inspectionRepository.GetByIdAsync(id);
        if (inspection == null) return false;

        var files = await _fileRepository.GetByInspectionIdAsync(id);
        var fileList = files?.ToList() ?? new List<InspectionFile>();

        if (fileList.Any())
        {
            foreach (var file in fileList)
            {
                try
                {
                    await _storageService.DeleteFileAsync(_bucketName, file.StoredFileName);
                }
                catch
                {
                    continue;
                }
            }

            foreach (var file in fileList)
            {
                file.IsDeleted = true;
                file.DeletedAt = DateTime.UtcNow;
                _context.InspectionFiles.Update(file);
            }
        }

        _inspectionRepository.Delete(inspection);
        await _context.SaveChangesAsync(); // 

        return true;
    }

    public List<string> GetAllStatuses()
    {
        return Enum.GetNames(typeof(InspectionStatus)).ToList();
    }

    private static InspectionDto ToDto(Inspection inspection)
    {
        return new InspectionDto
        {
            Id = inspection.Id,
            ProductName = inspection.ProductName,
            Description = inspection.Description,
            InspectorName = inspection.InspectorName,
            Status = inspection.Status.ToString(),
            CreatedAt = inspection.CreatedAt,
            UpdatedAt = inspection.UpdatedAt ?? inspection.CreatedAt
        };
    }
}