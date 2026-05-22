using GarageField.Services.StorageServices;
using Microsoft.Extensions.Configuration; // 🚀 IConfiguration için eksik olan using eklendi
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace GarageField.Services.StorageServices
{
    public class BucketService
    {
        private readonly IFileStorageService _storage;
        private readonly string _bucket;

        public BucketService(IFileStorageService storage, IConfiguration config)
        {
            _storage = storage;
            _bucket = config["GarageSettings:BucketName"]!;
        }

        public async Task<byte[]> DownloadEntireBucketAsync()
        {
            var keys = await _storage.ListFilesAsync(_bucket);

            using var ms = new MemoryStream();

            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                if (keys != null && keys.Any())
                {
                    foreach (var key in keys)
                    {
                        try
                        {
                            using var stream = await _storage.DownloadFileAsync(_bucket, key);
                            if (stream == null) continue;

                          
                            var entry = zip.CreateEntry(key, CompressionLevel.Fastest);

                            using var entryStream = entry.Open();
                            await stream.CopyToAsync(entryStream);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }

            return ms.ToArray();
        }
    }
}