using GarageField.Services.InspectionFileServices;
using Microsoft.AspNetCore.Mvc;

namespace GarageField.Controllers.InspectionFile
{
    [ApiController]
    [Route("api/inspections/{inspectionId:guid}/files")]
    public class InspectionFilesController : ControllerBase
    {
        private readonly InspectionFileService _service;

        public InspectionFilesController(InspectionFileService service)
        {
            _service = service;
        }

        // UPLOAD
        [HttpPost]
        public async Task<IActionResult> Upload(Guid inspectionId, [FromForm] List<IFormFile> files)
        {
            var result = await _service.UploadFilesAsync(inspectionId, files);
            return Ok(result);
        }

        // GET FILES
        [HttpGet]
        public async Task<IActionResult> GetFiles(Guid inspectionId)
        {
            return Ok(await _service.GetFilesAsync(inspectionId));
        }

        // DOWNLOAD SINGLE
        [HttpGet("{fileId:guid}/download")]
        public async Task<IActionResult> Download(Guid inspectionId, Guid fileId)
        {
            var result = await _service.DownloadFileAsync(inspectionId, fileId);

            if (result == null)
                return NotFound();

            return File(result.Value.Item1, result.Value.Item2, result.Value.Item3);
        }

        // DELETE FILE
        [HttpDelete("{fileId:guid}")]
        public async Task<IActionResult> Delete(Guid inspectionId, Guid fileId)
        {
            var success = await _service.DeleteFileAsync(inspectionId, fileId);

            if (!success)
                return NotFound();

            return NoContent();
        }

        // ZIP DOWNLOAD
        [HttpGet("download-all")]
        public async Task<IActionResult> DownloadAll(Guid inspectionId)
        {
            var result = await _service.ExportFilesAsZipAsync(inspectionId);

            if (result == null)
                return NotFound();

            return File(
                result.Value.ZipBytes,
                "application/zip",
                result.Value.FileName
            );
        }
    }
}