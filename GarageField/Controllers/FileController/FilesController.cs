using GarageField.Services.InspectionFileServices;
using GarageField.Services.StorageServices;
using GarageField.Services.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GarageField.Controllers.File;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly BucketService _bucketService;
    private readonly IFileStorageService _storage;
    private readonly string _bucket;

    public FilesController(
        IFileStorageService storage,
        BucketService bucketService,
        IConfiguration config)
    {
        _storage = storage;
        _bucketService = bucketService;
        _bucket = config["GarageSettings:BucketName"]!;
    }

    [HttpGet("download-bucket")]
    public async Task<IActionResult> DownloadBucket()
    {
        var zip = await _bucketService.DownloadEntireBucketAsync();

        return File(zip, "application/zip", "garage-backup.zip", enableRangeProcessing: true);
    }

  
}