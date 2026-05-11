using GarageField.Data;
using GarageField.DTOs.Inspection;
using GarageField.Entities;
using GarageField.Enums;
using Microsoft.EntityFrameworkCore;

namespace GarageField.Services.InspectionServices
{
    public class InspectionService
    {
        private readonly AppDbContext _context;

        public InspectionService(AppDbContext context)
        {
            _context = context;
        }

        // 🔥 CREATE
        public async Task<InspectionDto> CreateAsync(CreateInspectionDto dto)
        {
            var entity = ToEntity(dto);

            _context.Inspections.Add(entity);
            await _context.SaveChangesAsync();

            return ToDto(entity);
        }

        // 🔥 GET ALL
        public async Task<List<InspectionDto>> GetAllAsync()
        {
            var entities = await _context.Inspections.ToListAsync();

            return entities.Select(ToDto).ToList();
        }

        // 🔥 GET BY ID
        public async Task<InspectionDto?> GetByIdAsync(Guid id)
        {
            var entity = await _context.Inspections.FindAsync(id);

            if (entity == null)
                return null;

            return ToDto(entity);
        }

        // 🔥 UPDATE (FULL)
        public async Task<bool> UpdateAsync(Guid id, CreateInspectionDto dto)
        {
            var entity = await _context.Inspections.FindAsync(id);

            if (entity == null)
                return false;

            UpdateEntity(entity, dto);

            await _context.SaveChangesAsync();

            return true;
        }

        // 🔥 PATCH STATUS
        public async Task<bool> UpdateStatusAsync(Guid id, string status)
        {
            var entity = await _context.Inspections.FindAsync(id);

            if (entity == null)
                return false;

            entity.Status = ParseStatus(status);
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // 🔥 DELETE
        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Inspections.FindAsync(id);

            if (entity == null)
                return false;

            _context.Inspections.Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }

        // 🔥 STATUSES
        public List<string> GetAllStatuses()
        {
            return Enum.GetNames(typeof(InspectionStatus)).ToList();
        }

        // ======================
        // 🔥 MAPPING METHODS
        // ======================

        private Inspection ToEntity(CreateInspectionDto dto)
        {
            return new Inspection
            {
                Id = Guid.NewGuid(),
                ProductName = dto.ProductName,
                Description = dto.Description,
                InspectorName = dto.InspectorName,
                Status = ParseStatus(dto.Status),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private void UpdateEntity(Inspection entity, CreateInspectionDto dto)
        {
            entity.ProductName = dto.ProductName;
            entity.Description = dto.Description;
            entity.InspectorName = dto.InspectorName;
            entity.Status = ParseStatus(dto.Status);
            entity.UpdatedAt = DateTime.UtcNow;
        }

        private InspectionDto ToDto(Inspection entity)
        {
            return new InspectionDto
            {
                Id = entity.Id,
                ProductName = entity.ProductName,
                Description = entity.Description,
                InspectorName = entity.InspectorName,
                Status = entity.Status.ToString(),
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        // 🔥 SAFE ENUM PARSE
        private InspectionStatus ParseStatus(string status)
        {
            if (!Enum.TryParse<InspectionStatus>(status, true, out var parsed))
                throw new ArgumentException($"Invalid status: {status}");

            return parsed;
        }
    }
}