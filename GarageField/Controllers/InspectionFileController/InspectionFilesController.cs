using GarageField.Services.InspectionFileServices; // ✅ Senkronize edilmiş doğru namespace
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GarageField.Controllers;

[ApiController]
[Route("api/inspections/{inspectionId:guid}/files")]
public class InspectionFilesController : ControllerBase
{
    private readonly InspectionFileService _service;

    public InspectionFilesController(InspectionFileService service)
    {
        _service = service;
    }

    // 🚀 1. UPLOAD (BÜYÜK DOSYA LİMİTLERİ KALDIRILDI)
    [RequestSizeLimit(long.MaxValue)]
    [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
    [HttpPost]
    public async Task<IActionResult> Upload(Guid inspectionId, [FromForm] List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return BadRequest("No files uploaded");

        var result = await _service.UploadFilesAsync(inspectionId, files);
        return Ok(result);
    }

    // 🚀 2. GET FILES (Muayeneye Özel Dosyaları Listele)
    [HttpGet]
    public async Task<IActionResult> GetFiles(Guid inspectionId)
    {
        var result = await _service.GetFilesAsync(inspectionId);
        return Ok(result);
    }

    // 🚀 3. GET ALL FILES (Sistemdeki Tüm Dosyaları Listele)
    [HttpGet("/api/files/all")]
    public async Task<IActionResult> GetAllFiles()
    {
        var result = await _service.GetAllFilesAsync();
        return Ok(result);
    }

    // 🚀 4. DOWNLOAD SINGLE (Mükerrerlik Çözüldü - Tamamen RAM Dostu Stream)
    [HttpGet("{fileId:guid}/download")]
    public async Task<IActionResult> Download(Guid inspectionId, Guid fileId)
    {
        var result = await _service.DownloadFileAsync(inspectionId, fileId);
        if (result == null) return NotFound();

        // Eski byte[] kullanan metot silindi, burası artık MinIO ile birebir ikiz!
        return File(
            result.Value.Stream,       // Canlı veri akışı
            result.Value.ContentType,  // Dosya tipi
            result.Value.FileName,     // Dosya adı
            enableRangeProcessing: true
        );
    }

    // 🚀 5. DELETE
    [HttpDelete("{fileId:guid}")]
    public async Task<IActionResult> Delete(Guid inspectionId, Guid fileId)
    {
        var success = await _service.DeleteFileAsync(inspectionId, fileId);
        if (!success) return NotFound();

        return NoContent();
    }

    // 🚀 6. DOWNLOAD ALL ZIP (Performans Testleri İçin Canlı Akış Boru Hattı)
    [HttpGet("download-all")]
    public async Task<IActionResult> DownloadAll(Guid inspectionId)
    {
        // Sunucu RAM'ini şişirmemek için geçici bir disk boru hattı açıyoruz
        var tempFileStream = new FileStream(
            Path.GetTempFileName(),
            FileMode.Create,
            FileAccess.ReadWrite,
            FileShare.None,
            4096,
            FileOptions.DeleteOnClose
        );

        var zipStream = await _service.ExportFilesAsZipStreamAsync(inspectionId, tempFileStream);

        if (zipStream == null)
        {
            await tempFileStream.DisposeAsync();
            return NotFound("Bu muayeneye ait dosya bulunamadı.");
        }

        return File(
            zipStream,
            "application/zip",
            $"inspection_{inspectionId}.zip",
            enableRangeProcessing: true
        );
    }
}