namespace GarageField.Services.StorageServices
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(string bucket, string key, Stream stream, string contentType);

        Task<Stream> DownloadFileAsync(string bucket, string key);

        Task DeleteFileAsync(string bucket, string key);

        Task<List<string>> ListFilesAsync(string bucket);
    }
}