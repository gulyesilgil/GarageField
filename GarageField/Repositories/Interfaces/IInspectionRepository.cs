using EF.Core.Repository.Interface.Repository; // ✅ Artık paket yüklendiği için hata vermeyecek
using GarageField.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GarageField.Repositories.Interfaces;

public interface IInspectionRepository : ICommonRepository<Inspection>
{
    Task<Inspection?> GetByIdAsync(Guid id);
    Task<List<Inspection>> GetAllAsync();
    Task InsertManyAsync(List<Inspection> inspections); // ✅ MinIO'daki ismin birebir aynısı
}