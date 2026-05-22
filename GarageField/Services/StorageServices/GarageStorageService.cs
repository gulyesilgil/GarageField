using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using GarageField.Services.StorageServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace GarageField.Services.Storage;

public class GarageStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3;

    public GarageStorageService(IAmazonS3 s3)
    {
        _s3 = s3;
    }

    public async Task<string> UploadFileAsync(string bucket, string key, Stream stream, string contentType)
    {
        stream.Position = 0;

        string extension = Path.GetExtension(key);

        string uniqueKey = key;

        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(key);
        if (!Guid.TryParse(fileNameWithoutExt, out _))
        {
            uniqueKey = $"{Guid.NewGuid()}{extension}";
        }

        var request = new PutObjectRequest
        {
            BucketName = bucket,
            Key = uniqueKey, // Artık sadece ve sadece saf GUID + uzantı!
            InputStream = stream,
            ContentType = contentType,
            UseChunkEncoding = false
        };

        await _s3.PutObjectAsync(request);

        return uniqueKey;
    }
    public async Task<Stream> DownloadFileAsync(string bucket, string objectName)
    {
        var response = await _s3.GetObjectAsync(bucket, objectName);
        return response.ResponseStream;
    }

    public async Task DeleteFileAsync(string bucket, string objectName)
    {
        await _s3.DeleteObjectAsync(bucket, objectName);
    }

    public async Task<List<string>> ListFilesAsync(string bucket)
    {
        var result = new List<string>();
        string? continuationToken = null;
        ListObjectsV2Response? response = null;

        do
        {
            var request = new ListObjectsV2Request
            {
                BucketName = bucket,
                ContinuationToken = continuationToken
            };

            response = await _s3.ListObjectsV2Async(request);

            if (response != null && response.S3Objects != null)
            {
                result.AddRange(response.S3Objects.Select(x => x.Key));
            }

            continuationToken = (response != null && response.IsTruncated == true) ? response.NextContinuationToken : null;

        } while (continuationToken != null);

        return result;
    }

    public async Task<Stream?> ExportFilesAsZipStreamAsync(string bucket, List<(string FileKey, string OriginalName)> filesToZip, Stream destinationStream)
    {
        if (filesToZip == null || !filesToZip.Any())
            return null;

        using (var archive = new ZipArchive(destinationStream, ZipArchiveMode.Create, true))
        {
            foreach (var file in filesToZip)
            {
                try
                {
                    var fileStream = await DownloadFileAsync(bucket, file.FileKey);
                    if (fileStream == null) continue;

                    var entry = archive.CreateEntry(file.FileKey, CompressionLevel.Fastest);

                    using (var entryStream = entry.Open())
                    {
                        await fileStream.CopyToAsync(entryStream);
                    }

                    await fileStream.DisposeAsync();
                }
                catch
                {
                    continue;
                }
            }
        }

        destinationStream.Position = 0;
        return destinationStream;
    }
}