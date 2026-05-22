using GarageField.DTOs.InspectionFile;
using GarageField.Entities;
using GarageField.Repositories.Interfaces;
using GarageField.Services.StorageServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace GarageField.Services.InspectionFileServices;

public class InspectionFileService
{
    private readonly IFileStorageService _storageService;
    private readonly string _bucketName;
    private readonly IInspectionRepository _inspectionRepository;
    private readonly IInspectionFileRepository _fileRepository;

    public InspectionFileService(
        IFileStorageService storageService,
        IConfiguration config,
        IInspectionRepository inspectionRepository,
        IInspectionFileRepository fileRepository)
    {
        _storageService = storageService;
        _bucketName = config["GarageSettings:BucketName"] ?? "garage-bucket";
        _inspectionRepository = inspectionRepository;
        _fileRepository = fileRepository;
    }

    public async Task<List<InspectionFileDto>> UploadFilesAsync(Guid inspectionId, List<IFormFile> files)
    {
        var inspection = await _inspectionRepository.GetByIdAsync(inspectionId);
        if (inspection == null) return new List<InspectionFileDto>();

        var entities = new List<InspectionFile>();

        foreach (var file in files)
        {
            if (file.Length == 0) continue;

            using var stream = file.OpenReadStream();

            var finalStoredName = await _storageService.UploadFileAsync(
                _bucketName,
                file.FileName,
                stream,
                file.ContentType
            );

            var entity = new InspectionFile
            {
                Id = Guid.NewGuid(),
                InspectionId = inspectionId,
                FileName = file.FileName,
                StoredFileName = finalStoredName, // Artık iki taraf da milimetrik olarak aynı ismi biliyor!
                ContentType = file.ContentType,
                FileSize = file.Length,
                BucketName = _bucketName,
                CreatedAt = DateTime.UtcNow
            };

            entities.Add(entity);
        }

        await _fileRepository.InsertManyAsync(entities);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<InspectionFileDto>> GetAllFilesAsync()
    {
        var files = await _fileRepository.GetAllAsync();
        return files.Select(MapToDto).ToList();
    }

    public async Task<List<InspectionFileDto>> GetFilesAsync(Guid inspectionId)
    {
        var files = await _fileRepository.GetByInspectionIdAsync(inspectionId);
        return files.Select(MapToDto).ToList();
    }

    public async Task<(Stream Stream, string ContentType, string FileName)?> DownloadFileAsync(Guid inspectionId, Guid fileId)
    {
        var file = await _fileRepository.GetByIdAndInspectionIdAsync(inspectionId, fileId);
        if (file == null) return null;

        var stream = await _storageService.DownloadFileAsync(
            file.BucketName,
            file.StoredFileName
        );

        return (stream, file.ContentType, file.FileName);
    }

    public async Task<bool> DeleteFileAsync(Guid inspectionId, Guid fileId)
    {
        var file = await _fileRepository.GetByIdAndInspectionIdAsync(inspectionId, fileId);
        if (file == null) return false;

        await _storageService.DeleteFileAsync(file.BucketName, file.StoredFileName);

        // Veritabanından (PostgreSQL soft-delete) uçurma
        await _fileRepository.DeleteAsync(file);

        return true;
    }

    public async Task<Stream?> ExportFilesAsZipStreamAsync(Guid inspectionId, Stream outputStream)
    {
        var files = await _fileRepository.GetByInspectionIdAsync(inspectionId);
        if (files == null || files.Count == 0) return null;

        using (var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, true))
        {
            foreach (var file in files)
            {
                try
                {
                    using var stream = await _storageService.DownloadFileAsync(file.BucketName, file.StoredFileName);
                    if (stream == null) continue;

                    var compression = file.FileSize > 50 * 1024 * 1024
                        ? CompressionLevel.NoCompression
                        : CompressionLevel.Fastest;

                    var entry = archive.CreateEntry(file.FileName, compression);

                    using var entryStream = entry.Open();
                    await stream.CopyToAsync(entryStream);
                }
                catch
                {
                    continue;
                }
            }
        }

        outputStream.Position = 0;
        return outputStream;
    }

    private InspectionFileDto MapToDto(InspectionFile file)
    {
        return new InspectionFileDto
        {
            Id = file.Id,
            InspectionId = file.InspectionId,
            FileName = file.FileName,
            FileSize = file.FileSize,
            ContentType = file.ContentType,
            CreatedAt = file.CreatedAt
        };
    }
}