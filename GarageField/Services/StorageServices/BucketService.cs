using GarageField.Services.StorageServices;
using System.IO.Compression;

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

            if (!keys.Any())
                throw new Exception("Bucket boş");

            using var ms = new MemoryStream();

            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var key in keys)
                {
                    try
                    {
                        using var stream = await _storage.DownloadFileAsync(_bucket, key);

                        var entry = zip.CreateEntry(key);

                        using var entryStream = entry.Open();
                        await stream.CopyToAsync(entryStream);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            return ms.ToArray();
        }
    }
}