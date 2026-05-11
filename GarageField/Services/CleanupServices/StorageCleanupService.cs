using GarageField.Data;
using GarageField.Services.StorageServices;
using Microsoft.EntityFrameworkCore;

namespace GarageField.Services.CleanupServices
{
    public class StorageCleanupService
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _storage;
        private readonly string _bucket;

        public StorageCleanupService(
            AppDbContext context,
            IFileStorageService storage,
            IConfiguration config)
        {
            _context = context;
            _storage = storage;
            _bucket = config["GarageSettings:BucketName"]!;
        }

        public async Task<List<string>> CleanupAsync(bool dryRun)
        {
            var storageFiles = await _storage.ListFilesAsync(_bucket);

            var dbFiles = await _context.InspectionFiles
                .Select(x => x.StoredFileName)
                .ToListAsync();

            var orphans = storageFiles
                .Where(x => !dbFiles.Contains(x))
                .ToList();

            if (!dryRun)
            {
                foreach (var key in orphans)
                {
                    await _storage.DeleteFileAsync(_bucket, key);
                }
            }

            return orphans;
        }
    }
}