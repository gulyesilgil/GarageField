using EF.Core.Repository.Interface.Repository;
using GarageField.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GarageField.Repositories.Interfaces;

public interface IInspectionFileRepository : ICommonRepository<InspectionFile>
{
    Task<List<InspectionFile>> GetByInspectionIdAsync(Guid inspectionId);
    Task<List<InspectionFile>> GetAllAsync();
    Task<InspectionFile?> GetByIdAndInspectionIdAsync(Guid inspectionId, Guid fileId);
    Task InsertManyAsync(List<InspectionFile> inspectionFiles);
    Task DeleteAsync(InspectionFile file); // ✅ Servisin beklediği silme imzası
}