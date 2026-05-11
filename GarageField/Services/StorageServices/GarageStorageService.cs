using Amazon.S3;
using Amazon.S3.Model;

namespace GarageField.Services.StorageServices
{
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

            var request = new PutObjectRequest
            {
                BucketName = bucket,
                Key = key,
                InputStream = stream,
                ContentType = contentType,
                UseChunkEncoding = false
            };

            await _s3.PutObjectAsync(request);

            return key;
        }

        public async Task<Stream> DownloadFileAsync(string bucket, string key)
        {
            var response = await _s3.GetObjectAsync(bucket, key);
            return response.ResponseStream;
        }

        public async Task DeleteFileAsync(string bucket, string key)
        {
            await _s3.DeleteObjectAsync(bucket, key);
        }

        public async Task<List<string>> ListFilesAsync(string bucket)
        {
            var result = new List<string>();

            var response = await _s3.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = bucket
            });

            foreach (var obj in response.S3Objects)
            {
                result.Add(obj.Key);
            }

            return result;
        }
    }
}