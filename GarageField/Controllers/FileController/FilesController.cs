using GarageField.Services.CleanupServices;
using GarageField.Services.StorageServices;
using Microsoft.AspNetCore.Mvc;

namespace GarageField.Controllers.File
{
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        private readonly StorageCleanupService _cleanupService;
        private readonly BucketService _bucketService;
        private readonly IFileStorageService _storage;
        private readonly string _bucket;

        public FilesController(
            StorageCleanupService cleanupService,
            IFileStorageService storage,
            BucketService bucketService,
            IConfiguration config)
        {
            _cleanupService = cleanupService;
            _storage = storage;
            _bucketService = bucketService;
            _bucket = config["GarageSettings:BucketName"]!;
        }

        // 🔥 ORPHAN CLEANUP
        [HttpGet("cleanup")]
        public async Task<IActionResult> Cleanup([FromQuery] bool dryRun = true)
        {
            var result = await _cleanupService.CleanupAsync(dryRun);

            return Ok(new
            {
                dryRun,
                orphanCount = result.Count,
                orphanFiles = result
            });
        }

        // 🔥 DOWNLOAD ENTIRE BUCKET
        [HttpGet("download-bucket")]
        public async Task<IActionResult> DownloadBucket()
        {
            var zip = await _bucketService.DownloadEntireBucketAsync();

            return File(zip, "application/zip", "garage-backup.zip");
        }

        // 🔥 DEBUG BUCKET
        [HttpGet("debug-bucket")]
        public async Task<IActionResult> DebugBucket()
        {
            var files = await _storage.ListFilesAsync(_bucket); // ✅ FIX

            return Ok(files);
        }
    }
}