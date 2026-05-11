using GarageField.Data;
using GarageField.DTOs.InspectionFile;
using GarageField.Entities;
using GarageField.Services.StorageServices;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

namespace GarageField.Services.InspectionFileServices
{
    public class InspectionFileService
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _storage;
        private readonly string _bucket;

        public InspectionFileService(
            AppDbContext context,
            IFileStorageService storage,
            IConfiguration config)
        {
            _context = context;
            _storage = storage;
            _bucket = config["GarageSettings:BucketName"]!;
        }

        public async Task<List<InspectionFileDto>> UploadFilesAsync(Guid inspectionId, List<IFormFile> files)
        {
            var result = new List<InspectionFileDto>();

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                var key = $"{Guid.NewGuid()}_{file.FileName}";

                using var stream = file.OpenReadStream();

                await _storage.UploadFileAsync(_bucket, key, stream, file.ContentType);

                var entity = new InspectionFile
                {
                    Id = Guid.NewGuid(),
                    InspectionId = inspectionId,
                    FileName = file.FileName,
                    StoredFileName = key,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    BucketName = _bucket,
                    CreatedAt = DateTime.UtcNow
                };

                _context.InspectionFiles.Add(entity);
                result.Add(ToDto(entity));
            }

            await _context.SaveChangesAsync();

            return result;
        }

        public async Task<List<InspectionFileDto>> GetFilesAsync(Guid inspectionId)
        {
            return await _context.InspectionFiles
                .Where(x => x.InspectionId == inspectionId)
                .Select(x => ToDto(x))
                .ToListAsync();
        }

        // 🔥 GET ALL FILES (GLOBAL)
        public async Task<List<AllInspectionFileDto>> GetAllFilesAsync()
        {
            return await _context.InspectionFiles
                .Select(f => new AllInspectionFileDto
                {
                    Id = f.Id,
                    InspectionId = f.InspectionId,
                    FileName = f.FileName,
                    FileSize = f.FileSize,
                    ContentType = f.ContentType,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<(byte[] FileBytes, string ContentType, string FileName)?> DownloadFileAsync(Guid inspectionId, Guid fileId)
        {
            var file = await _context.InspectionFiles
                .FirstOrDefaultAsync(x => x.Id == fileId && x.InspectionId == inspectionId);

            if (file == null) return null;

            using var stream = await _storage.DownloadFileAsync(file.BucketName, file.StoredFileName);

            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            return (ms.ToArray(), file.ContentType, file.FileName);
        }

        public async Task<bool> DeleteFileAsync(Guid inspectionId, Guid fileId)
        {
            var file = await _context.InspectionFiles
                .FirstOrDefaultAsync(x => x.Id == fileId && x.InspectionId == inspectionId);

            if (file == null) return false;

            await _storage.DeleteFileAsync(file.BucketName, file.StoredFileName);

            _context.InspectionFiles.Remove(file);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<(byte[] ZipBytes, string FileName)?> ExportFilesAsZipAsync(Guid inspectionId)
        {
            var files = await _context.InspectionFiles
                .Where(x => x.InspectionId == inspectionId)
                .ToListAsync();

            if (!files.Any()) return null;

            using var zipStream = new MemoryStream();

            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var file in files)
                {
                    try
                    {
                        using var stream = await _storage.DownloadFileAsync(file.BucketName, file.StoredFileName);

                        var entry = archive.CreateEntry(file.FileName);

                        using var entryStream = entry.Open();
                        await stream.CopyToAsync(entryStream);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            return (zipStream.ToArray(), $"inspection_{inspectionId}.zip");
        }

        private static InspectionFileDto ToDto(InspectionFile file)
        {
            return new InspectionFileDto
            {
                Id = file.Id,
                FileName = file.FileName,
                FileSize = file.FileSize,
                ContentType = file.ContentType,
                CreatedAt = file.CreatedAt
            };
        }
    }
}